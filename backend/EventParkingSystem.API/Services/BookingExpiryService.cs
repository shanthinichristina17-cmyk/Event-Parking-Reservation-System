using EventParkingSystem.API.Common;
using Microsoft.Extensions.Options;

namespace EventParkingSystem.API.Services;

public sealed class BookingExpiryService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly BookingSettings _settings;
    private readonly ILogger<BookingExpiryService> _logger;

    public BookingExpiryService(
        IServiceScopeFactory scopeFactory,
        IOptions<BookingSettings> settings,
        ILogger<BookingExpiryService> logger)
    {
        _scopeFactory = scopeFactory;
        _settings = settings.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var delaySeconds = Math.Max(10, _settings.ExpiryCheckSeconds);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var service = scope.ServiceProvider.GetRequiredService<IBookingService>();
                var count = await service.ExpirePendingHoldsAsync();

                if (count > 0)
                    _logger.LogInformation("Expired {Count} booking hold(s).", count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Booking expiry background service failed.");
            }

            try
            {
                await Task.Delay(TimeSpan.FromSeconds(delaySeconds), stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
        }
    }
}
