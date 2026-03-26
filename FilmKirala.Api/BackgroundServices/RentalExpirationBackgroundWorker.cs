using FilmKirala.Application.Interfaces.Services;

namespace FilmKirala.Api.BackgroundServices;

public class RentalExpirationBackgroundWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<RentalExpirationBackgroundWorker> _logger;
    private readonly TimeSpan _interval = TimeSpan.FromMinutes(10);

    public RentalExpirationBackgroundWorker(IServiceScopeFactory scopeFactory, ILogger<RentalExpirationBackgroundWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("[EXPIRATION-WORKER] Started | Interval: {Interval}min", _interval.TotalMinutes);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var rentalService = scope.ServiceProvider.GetRequiredService<IRentalService>();

                await rentalService.CheckExpiredRentalsAsync();

                _logger.LogInformation("[EXPIRATION-WORKER] Expired rental check completed at {Time}", DateTime.UtcNow);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[EXPIRATION-WORKER] Error during expired rental check.");
            }
            await Task.Delay(_interval, stoppingToken);
        }
    }
}
