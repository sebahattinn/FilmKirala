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
    private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(4);  // maks 4 rapor aynı anda işlenebilir. Fazla gelirse kuyrukta bekler
    private int _processedCount = 0;                                     // Toplam işlenen rapor sayacı

    public ReportBackgroundWorker(IServiceScopeFactory scopeFactory, ILogger<ReportBackgroundWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(">>> [REPORT-WORKER] Started | Polling: {Interval}s | MaxConcurrency: 3", _pollInterval.Seconds);

        // Delay startup polling so the app and DB settle before heavy report queries begin.
        await Task.Delay(TimeSpan.FromSeconds(15), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            bool semaphoreAcquired = false;
            try
            {
                _logger.LogDebug("[HEARTBEAT] {Time} - Looking for pending work... (Active Slots: {Slots}/3)",
                    DateTime.Now.ToString("HH:mm:ss"), 3 - _semaphore.CurrentCount);

                await _semaphore.WaitAsync(stoppingToken);
                semaphoreAcquired = true;

                await ProcessNextPendingJobAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                // If ProcessNextPendingJobAsync threw before spawning Task.Run,
                // the semaphore was never released inside it — release it here.
                if (semaphoreAcquired)
                    _semaphore.Release();

                _logger.LogCritical(ex, "[FATAL ERROR] The main loop of the BackgroundWorker has crashed.!");
            }

            await Task.Delay(_pollInterval, stoppingToken);
        }
    }

    private async Task ProcessNextPendingJobAsync(CancellationToken stoppingToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // DB error here propagates to ExecuteAsync's catch, which releases the semaphore.
        var pendingJob = await db.ReportJobs
            .AsNoTracking()
            .Where(j => j.Status == ReportJobStatus.Pending)
            .OrderBy(j => j.CreatedAt)
            .FirstOrDefaultAsync(stoppingToken);

        if (pendingJob == null)
        {
            // If there is no work, we return the slot we reserved.
            _semaphore.Release();
            return;
        }

        _logger.LogWarning("[NEW JOB]  Pending report caught! JobId: {JobId}", pendingJob.JobId);

        // I am initiating tracking through a new scope to update the status to “Processing.”
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

        // I am starting the background task.
        _ = Task.Run(async () =>
        {
            _logger.LogInformation("[START]  Processing: {JobId} (Thread: {ThreadId})", jobId, Environment.CurrentManagedThreadId);
            var startTime = DateTime.Now;

            try
            {
                using var jobScope = _scopeFactory.CreateScope();
                var reportService = jobScope.ServiceProvider.GetRequiredService<IReportService>();

                // The service call that does the heavy lifting
                await reportService.CreateLargeReportInBackgroundAsync(jobId, isCsv);

                var duration = DateTime.Now - startTime;
                Interlocked.Increment(ref _processedCount);
                _logger.LogInformation("[SUCCESS]  Completed: {JobId} | Süre: {Sec}sn | Toplam: {Count}",
                    jobId, Math.Round(duration.TotalSeconds, 2), _processedCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ERROR]  Boombed: {JobId} | Mesaj: {Msg}", jobId, ex.Message);

                try
                {
                    using var failScope = _scopeFactory.CreateScope();
                    var failDb = failScope.ServiceProvider.GetRequiredService<AppDbContext>();
                    var failedJob = await failDb.ReportJobs.FirstOrDefaultAsync(j => j.JobId == jobId);
                    if (failedJob != null)
                    {
                        failedJob.MarkAsFailed();
                        await failDb.SaveChangesAsync();
                        _logger.LogInformation("[DB UPDATE]  {JobId} The status has been updated to ‘Failed’.", jobId);
                    }
                }
                catch (Exception dbEx)
                {
                    _logger.LogError(dbEx, "[CRITICAL] The database update failed.! JobId: {JobId}", jobId);
                }
            }
            finally
            {
                _semaphore.Release(); 
                _logger.LogDebug("[RELEASE]  The slot is empty. Remaining active jobs: {Count}/3", 3 - _semaphore.CurrentCount);
            }
        }, stoppingToken);
    }
}