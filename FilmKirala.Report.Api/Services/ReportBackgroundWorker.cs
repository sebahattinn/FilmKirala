using FilmKirala.Infrastructure.Persistence;
using FilmKirala.Report.Api.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FilmKirala.Report.Api.Services;

public class ReportBackgroundWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ReportBackgroundWorker> _logger;
    private readonly TimeSpan _interval = TimeSpan.FromSeconds(45);
    private readonly int _queryTimeout = 60; // 10 saniye çok azdı, 60 yaptık.

    public ReportBackgroundWorker(IServiceScopeFactory scopeFactory, ILogger<ReportBackgroundWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("╔════════════════════════════════════════╗");
        _logger.LogInformation("║   🚀 ReportBackgroundWorker BAŞLADI     ║");
        _logger.LogInformation("║   ⏱️  Interval: {Interval} saniye         ║", _interval.Seconds);
        _logger.LogInformation("║   ⌛ Query Timeout: {Timeout} saniye     ║", _queryTimeout);
        _logger.LogInformation("╚════════════════════════════════════════╝");

        while (!stoppingToken.IsCancellationRequested)
        {
            var startTime = DateTime.Now;

            // 1. ADIM: KUYRUKTAN İŞ ÇEK (Multi-tasking için Task.Run kullanıldı)
            try
            {
                if (ReportService.TryDequeue(out var job))
                {
                    _logger.LogInformation(" [KUYRUK] İş bulundu! JobId: {JobId} başlatılıyor...", job.JobId);

                    // Ana döngüyü bloklamasın diye Task.Run ile ayırıyoruz.
                    _ = Task.Run(async () =>
                    {
                        using var scope = _scopeFactory.CreateScope();
                        var reportService = scope.ServiceProvider.GetRequiredService<IReportService>();
                        await reportService.CreateLargeReportInBackgroundAsync(job.JobId, job.IsCsv);
                    }, stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, " Kuyruktaki rapor işlenirken hata!");
            }

            // 2. ADIM: PERİYODİK DB KONTROLÜ
            try
            {
                await CheckDatabaseWithSafety(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, " Periyodik DB kontrol hatası!");
            }

            var duration = DateTime.Now - startTime;
            _logger.LogInformation(" [DÖNGÜ] Bitti. Süre: {Duration:F0}ms | ⏰ {Time}",
                duration.TotalMilliseconds, DateTime.Now.ToString("HH:mm:ss"));

            await Task.Delay(_interval, stoppingToken);
        }
    }

    private async Task CheckDatabaseWithSafety(CancellationToken stoppingToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
        cts.CancelAfter(TimeSpan.FromSeconds(_queryTimeout));

        try
        {
            var totalCount = await db.Rentals.AsNoTracking().CountAsync(cts.Token);

            _logger.LogInformation("╔════════════════════════════════════════╗");
            _logger.LogInformation("║  📊 DB KONTROL - {Time}           ║", DateTime.Now.ToString("HH:mm:ss"));
            _logger.LogInformation("║  📦 Toplam Kiralama: {Count,-10}      ║", totalCount);
            _logger.LogInformation("╚════════════════════════════════════════╝");
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning(" [UYARI] DB Kontrolü zaman aşımına uğradı!");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, " DB okuma hatası!");
        }
    }
}