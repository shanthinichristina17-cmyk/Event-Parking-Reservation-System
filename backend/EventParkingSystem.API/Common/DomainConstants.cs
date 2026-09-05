namespace EventParkingSystem.API.Common;

public static class Roles
{
    public const string Customer = "Customer";
    public const string Admin = "Admin";
}

public static class CustomerStatuses
{
    public const string Active = "Active";
    public const string Deactivated = "Deactivated";
}

public static class SeatStatuses
{
    public const string Available = "Available";
    public const string Held = "Held";
    public const string Booked = "Booked";
}

public static class ParkingStatuses
{
    public const string Available = "Available";
    public const string Held = "Held";
    public const string Reserved = "Reserved";
}

public static class BookingStatuses
{
    public const string Pending = "Pending";
    public const string Confirmed = "Confirmed";
    public const string Cancelled = "Cancelled";
    public const string Expired = "Expired";
}

public static class PaymentStatuses
{
    public const string Completed = "Completed";
}

public static class NotificationTypes
{
    public const string Confirmation = "Confirmation";
    public const string Cancellation = "Cancellation";
    public const string Payment = "Payment";
    public const string Reminder = "Reminder";
    public const string Update = "Update";
}
