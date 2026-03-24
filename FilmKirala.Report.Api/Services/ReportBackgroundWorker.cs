using System.Collections.Concurrent;
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
    private readonly IServiceScopeFactory _scopeFactory;                   // The pending jobs in the queue database table here
    private readonly ILogger<ReportBackgroundWorker> _logger;             // This page is constantly polling in the background, bro.
    private readonly TimeSpan _pollInterval = TimeSpan.FromSeconds(5);   // The worker checks every 5 seconds to see if there is a job available 
    private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(5);  // Aynı anda 1 rapor — XLSX 1M satır için yüzlerce MB RAM + ağır DB sorgusu, concurrent çalışınca SQL Server'ı boğuyor.
    private int _processedCount = 0;                                     //  Total number of processed reports
    private readonly ConcurrentDictionary<Guid, Task> _activeJobs = new(); // Tracks running jobs for graceful shutdown
    

    public ReportBackgroundWorker(IServiceScopeFactory scopeFactory, ILogger<ReportBackgroundWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;   
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(">>> [REPORT-WORKER] Started | Polling: {Interval}s | MaxConcurrency: {Max}",
            _pollInterval.Seconds, _semaphore.CurrentCount);

        
        await RecoverStuckJobsAsync(stoppingToken);
        await Task.Delay(TimeSpan.FromSeconds(3), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var availableSlots = _semaphore.CurrentCount;

                _logger.LogDebug("[HEARTBEAT] {Time} - Available slots: {Slots}/{Max}",
                    DateTime.UtcNow.ToString("HH:mm:ss"), availableSlots, _semaphore.CurrentCount + (4 - _semaphore.CurrentCount));

                if (availableSlots == 0)
                {
                    // All slots busy — short wait then re-check instead of full poll interval
                    await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
                    continue;
                }

                var pendingJobs = await FetchPendingJobsAsync(availableSlots, stoppingToken);

                if (pendingJobs.Count == 0)
                {
                    // No work — wait the full poll interval before hitting the DB again
                    await Task.Delay(_pollInterval, stoppingToken);
                    continue;
                }

                // Dispatch all fetched jobs in parallel — one Task.Run per job
                foreach (var (jobId, isCsv) in pendingJobs)
                {
                    await _semaphore.WaitAsync(stoppingToken);
                    DispatchJob(jobId, isCsv, stoppingToken);
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "[FATAL ERROR] The main loop of the BackgroundWorker has crashed!");
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await base.StopAsync(cancellationToken); // signals stoppingToken, waits for ExecuteAsync to exit

        var running = _activeJobs.Values.ToArray();
        if (running.Length > 0)
        {
            _logger.LogInformation("[SHUTDOWN] Waiting for {Count} active job(s) to finish...", running.Length);
            await Task.WhenAll(running);
            _logger.LogInformation("[SHUTDOWN] All active jobs completed.");
        }
    }
    private async Task RecoverStuckJobsAsync(CancellationToken stoppingToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var stuckJobs = await db.ReportJobs
            .Where(j => j.Status == ReportJobStatus.Processing)
            .ToListAsync(stoppingToken);

        if (stuckJobs.Count == 0) return;

        foreach (var job in stuckJobs)
            job.MarkAsPending();

        await db.SaveChangesAsync(stoppingToken);
        _logger.LogWarning("[RECOVERY] {Count} stuck job(s) reset to Pending: [{Ids}]",
            stuckJobs.Count, string.Join(", ", stuckJobs.Select(j => j.JobId)));
    }

    private async Task<List<(string JobId, bool IsCsv)>> FetchPendingJobsAsync(int limit, CancellationToken stoppingToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var pendingJobs = await db.ReportJobs
            .Where(j => j.Status == ReportJobStatus.Pending)
            .OrderBy(j => j.CreatedAt)
            .Take(limit)
            .ToListAsync(stoppingToken);

        if (pendingJobs.Count == 0)
            return [];

        foreach (var job in pendingJobs)
            job.MarkAsProcessing();

        await db.SaveChangesAsync(stoppingToken);

        _logger.LogWarning("[DISPATCH] Picked up {Count} pending job(s): [{Ids}]",
            pendingJobs.Count, string.Join(", ", pendingJobs.Select(j => j.JobId)));

        return pendingJobs.Select(j => (j.JobId, j.IsCsv)).ToList();
    }

    private void DispatchJob(string jobId, bool isCsv, CancellationToken stoppingToken)
    {
        var trackingId = Guid.NewGuid();

        var job = Task.Run(async () =>
        {
            _logger.LogInformation("[START] Processing: {JobId} (Thread: {ThreadId})", jobId, Environment.CurrentManagedThreadId);
            var startTime = DateTime.UtcNow;

            try
            {
                using var jobScope = _scopeFactory.CreateScope();
                var reportService = jobScope.ServiceProvider.GetRequiredService<IReportService>();

                await reportService.CreateLargeReportInBackgroundAsync(jobId, isCsv);

                var duration = DateTime.UtcNow - startTime;
                Interlocked.Increment(ref _processedCount);
                _logger.LogInformation("[SUCCESS] Completed: {JobId} | Duration: {Sec}s | Total: {Count}",
                    jobId, Math.Round(duration.TotalSeconds, 2), _processedCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ERROR] Failed: {JobId} | Message: {Msg}", jobId, ex.Message);

                try
                {
                    using var failScope = _scopeFactory.CreateScope();
                    var failDb = failScope.ServiceProvider.GetRequiredService<AppDbContext>();
                    var failedJob = await failDb.ReportJobs.FirstOrDefaultAsync(j => j.JobId == jobId);
                    if (failedJob != null)
                    {
                        failedJob.MarkAsFailed();
                        await failDb.SaveChangesAsync();
                    }
                }
                catch (Exception dbEx)
                {
                    _logger.LogError(dbEx, "[CRITICAL] DB update failed for JobId: {JobId}", jobId);
                }
            }
            finally
            {
                _semaphore.Release();
                _activeJobs.TryRemove(trackingId, out _);
                _logger.LogDebug("[RELEASE] Slot freed. Active jobs: {Active}/4", 4 - _semaphore.CurrentCount);
            }
        }, stoppingToken);

        _activeJobs.TryAdd(trackingId, job);
    }
}