using System.Text.Json;
using EventParkingSystem.API.Common;
using EventParkingSystem.API.DTOs;

namespace EventParkingSystem.API.Services;

public interface IAdminSettingsService
{
    Task<AdminSettingsResponse> GetAsync();
    Task<AdminSettingsResponse> UpdateAsync(UpdateAdminSettingsRequest request);
}

public sealed class AdminSettingsService : IAdminSettingsService
{
    private readonly string _filePath;
    private readonly IConfiguration _configuration;
    private readonly SemaphoreSlim _lock = new(1, 1);

    public AdminSettingsService(IWebHostEnvironment environment, IConfiguration configuration)
    {
        _configuration = configuration;
        _filePath = Path.Combine(AppContext.BaseDirectory, "admin-settings.json");
    }

    public async Task<AdminSettingsResponse> GetAsync()
    {
        await _lock.WaitAsync();
        try
        {
            if (!File.Exists(_filePath))
            {
                var defaults = CreateDefaults();
                await SaveInternalAsync(defaults);
                return defaults;
            }

            var json = await File.ReadAllTextAsync(_filePath);
            var value = JsonSerializer.Deserialize<AdminSettingsResponse>(json);
            return value ?? CreateDefaults();
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<AdminSettingsResponse> UpdateAsync(UpdateAdminSettingsRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.SiteName))
            throw ApiException.BadRequest("Site name is required.");

        if (string.IsNullOrWhiteSpace(request.Currency))
            throw ApiException.BadRequest("Currency is required.");

        if (request.HoldMinutes < 1 || request.HoldMinutes > 60)
            throw ApiException.BadRequest("HoldMinutes must be between 1 and 60.");

        var value = new AdminSettingsResponse(
            request.SiteName.Trim(),
            request.Currency.Trim().ToUpperInvariant(),
            request.HoldMinutes,
            request.AllowParking,
            request.EmailNotificationsEnabled,
            request.MaintenanceMode);

        await _lock.WaitAsync();
        try
        {
            await SaveInternalAsync(value);
            return value;
        }
        finally
        {
            _lock.Release();
        }
    }

    private AdminSettingsResponse CreateDefaults()
    {
        var configuredHold = _configuration.GetValue<int?>("Booking:HoldMinutes") ?? 10;
        var emailEnabled = _configuration.GetValue<bool?>("Email:Enabled") ?? false;

        return new AdminSettingsResponse(
            "Event Ease",
            "LKR",
            configuredHold,
            true,
            emailEnabled,
            false);
    }

    private Task SaveInternalAsync(AdminSettingsResponse value)
    {
        var json = JsonSerializer.Serialize(value, new JsonSerializerOptions
        {
            WriteIndented = true
        });
        return File.WriteAllTextAsync(_filePath, json);
    }
}
