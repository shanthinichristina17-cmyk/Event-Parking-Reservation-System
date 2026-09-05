namespace EventParkingSystem.API.Common;

public sealed class BookingSettings
{
    public int HoldMinutes { get; set; } = 10;
    public int ExpiryCheckSeconds { get; set; } = 30;
}
