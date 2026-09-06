namespace EventParkingSystem.API.Common;

public sealed class BookingSettings
{
    public int HoldMinutes { get; set; } = 5;
    public int ExpiryCheckSeconds { get; set; } = 30;
    public int CancellationCutoffHours { get; set; } = 24;
}
