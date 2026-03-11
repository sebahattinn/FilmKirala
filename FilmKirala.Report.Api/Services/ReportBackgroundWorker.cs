using FilmKirala.Domain.Enums;
using FilmKirala.Infrastructure.Persistence;
using FilmKirala.Report.Api.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FilmKirala.Report.Api.Services;

public class ReportBackgroundWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;                   // Buradaki kuyruk db tablosundaki pending olan job'lar
    private readonly ILogger<ReportBackgroundWorker> _logger;             // Arka planda sürekli polling yapıyo bu sayfa ab.
    private readonly TimeSpan _pollInterval = TimeSpan.FromSeconds(5);   // Worker Her 5 saniyede job var mı diye check ediyor 
    private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(3);  // maks 3 rapor aynı anda işlenebilir. Fazla gelirse kuyrukta bekler
    private int _processedCount = 0;                                     // Toplam işlenen rapor sayacı

    public ReportBackgroundWorker(IServiceScopeFactory scopeFactory, ILogger<ReportBackgroundWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(">>> [REPORT-WORKER] Started | Polling: {Interval}s | MaxConcurrency: 3", _pollInterval.Seconds);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                _logger.LogDebug("[HEARTBEAT] {Time} - Bekleyen iş aranıyor... (Active Slots: {Slots}/3)",
                    DateTime.Now.ToString("HH:mm:ss"), 3 - _semaphore.CurrentCount);

                // İşleme başlamadan önce bir slot bekliyoruz
                await _semaphore.WaitAsync(stoppingToken);

                await ProcessNextPendingJobAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "[FATAL ERROR] BackgroundWorker ana döngüsü patladı!");
            }

            await Task.Delay(_pollInterval, stoppingToken);
        }
    }

    private async Task ProcessNextPendingJobAsync(CancellationToken stoppingToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // deadlock önleme yeri
        var pendingJob = await db.ReportJobs
            .AsNoTracking()
            .Where(j => j.Status == ReportJobStatus.Pending)
            .OrderBy(j => j.CreatedAt)
            .FirstOrDefaultAsync(stoppingToken);

        if (pendingJob == null)
        {
            _semaphore.Release(); // İş yoksa tuttuğumuz slotu geri bırakıyoruz
            return;
        }

        _logger.LogWarning("[NEW JOB]  Bekleyen rapor yakalandı! JobId: {JobId}", pendingJob.JobId);

        // Durumu "Processing" olarak güncellemek için yeni bir scope üzerinden takip (tracking) başlatıyorum
        using (var updateScope = _scopeFactory.CreateScope())
        {
            var updateDb = updateScope.ServiceProvider.GetRequiredService<AppDbContext>();
            var jobToUpdate = await updateDb.ReportJobs.FirstOrDefaultAsync(x => x.Id == pendingJob.Id, stoppingToken);
            if (jobToUpdate != null)
            {
                jobToUpdate.MarkAsProcessing();
                await updateDb.SaveChangesAsync(stoppingToken);
            }
        }

        var jobId = pendingJob.JobId;
        var isCsv = pendingJob.IsCsv;

        // Arka plan görevini başlatıyorum
        _ = Task.Run(async () =>
        {
            _logger.LogInformation("[START]  İşleniyor: {JobId} (Thread: {ThreadId})", jobId, Environment.CurrentManagedThreadId);
            var startTime = DateTime.Now;

            try
            {
                using var jobScope = _scopeFactory.CreateScope();
                var reportService = jobScope.ServiceProvider.GetRequiredService<IReportService>();

                // Asıl ağır işi yapan servis çağrısı
                await reportService.CreateLargeReportInBackgroundAsync(jobId, isCsv);

                var duration = DateTime.Now - startTime;
                Interlocked.Increment(ref _processedCount);
                _logger.LogInformation("[SUCCESS]  Bitti: {JobId} | Süre: {Sec}sn | Toplam: {Count}",
                    jobId, Math.Round(duration.TotalSeconds, 2), _processedCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ERROR]  Patladı: {JobId} | Mesaj: {Msg}", jobId, ex.Message);

                try
                {
                    using var failScope = _scopeFactory.CreateScope();
                    var failDb = failScope.ServiceProvider.GetRequiredService<AppDbContext>();
                    var failedJob = await failDb.ReportJobs.FirstOrDefaultAsync(j => j.JobId == jobId);
                    if (failedJob != null)
                    {
                        failedJob.MarkAsFailed();
                        await failDb.SaveChangesAsync();
                        _logger.LogInformation("[DB UPDATE]  {JobId} durumu 'Failed' olarak güncellendi.", jobId);
                    }
                }
                catch (Exception dbEx)
                {
                    _logger.LogError(dbEx, "[CRITICAL] DB güncellemesi yapılamadı! JobId: {JobId}", jobId);
                }
            }
            finally
            {
                _semaphore.Release(); 
                _logger.LogDebug("[RELEASE]  Slot boşaldı. Kalan aktif iş: {Count}/3", 3 - _semaphore.CurrentCount);
            }
        }, stoppingToken);
    }
}