namespace EventParkingSystem.API.DTOs;

public record AdminDashboardResponse(
    int TotalEvents,
    int UpcomingEvents,
    int TotalCustomers,
    int ActiveCustomers,
    int TotalBookings,
    int PendingBookings,
    int ConfirmedBookings,
    int CancelledBookings,
    decimal TotalRevenue,
    int AvailableSeats,
    int HeldSeats,
    int BookedSeats,
    int AvailableParking,
    int HeldParking,
    int ReservedParking
);

public record AdminBookingDto(
    int BookingId,
    string BookingNumber,
    int CustomerId,
    string CustomerName,
    string CustomerEmail,
    int EventId,
    string EventName,
    string Status,
    DateTime? HoldExpiresAt,
    decimal TotalAmount,
    int SeatCount,
    string? ParkingSlot,
    string PaymentStatus,
    DateTime CreatedAt
);

public record AdminPaymentDto(
    int PaymentId,
    int BookingId,
    string BookingNumber,
    string CustomerName,
    string CustomerEmail,
    string EventName,
    decimal Amount,
    string Status,
    string ReceiptNumber,
    DateTime PaidAt
);

public record CustomerDashboardResponse(
    int CustomerId,
    int TotalBookings,
    int PendingBookings,
    int ConfirmedBookings,
    int UpcomingBookings,
    decimal TotalSpent,
    int UnreadNotifications,
    List<AdminBookingDto> RecentBookings
);

public record ReportSummaryResponse(
    DateTime? From,
    DateTime? To,
    int TotalBookings,
    int ConfirmedBookings,
    int CancelledBookings,
    int PendingBookings,
    decimal Revenue,
    decimal AverageBookingValue,
    int SeatsSold,
    int ParkingReservations
);

public record RevenueByEventDto(
    int EventId,
    string EventName,
    int ConfirmedBookings,
    decimal Revenue
);

public record BookingStatusDto(
    string Status,
    int Count
);

public record BroadcastNotificationRequest(
    string Type,
    string Message
);

public record AdminSettingsResponse(
    string SiteName,
    string Currency,
    int HoldMinutes,
    bool AllowParking,
    bool EmailNotificationsEnabled,
    bool MaintenanceMode
);

public record UpdateAdminSettingsRequest(
    string SiteName,
    string Currency,
    int HoldMinutes,
    bool AllowParking,
    bool EmailNotificationsEnabled,
    bool MaintenanceMode
);
