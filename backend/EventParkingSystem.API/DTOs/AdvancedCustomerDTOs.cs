namespace EventParkingSystem.API.DTOs;

public record CustomerNextBookingItem(
    int BookingId,
    string BookingNumber,
    int EventId,
    string EventName,
    DateOnly EventDate,
    TimeOnly StartTime,
    string VenueName,
    int SeatCount,
    string? ParkingSlot,
    decimal TotalAmount
);

public record CustomerAdvancedDashboardResponse(
    int TotalBookings,
    int ConfirmedBookings,
    int PendingBookings,
    int CancelledBookings,
    int UpcomingBookings,
    decimal TotalSpent,
    decimal TotalRefunded,
    decimal PromoSavings,
    int SeatsBooked,
    int ParkingBookings,
    int UnreadNotifications,
    string FavoriteCategory,
    CustomerNextBookingItem? NextBooking
);

public record CustomerMonthlySpendingItem(
    int Year,
    int Month,
    string Label,
    int BookingCount,
    decimal Spent,
    decimal Refunds,
    decimal NetSpent,
    decimal PromoSavings
);

public record RecommendedEventItem(
    int EventId,
    string EventName,
    DateOnly EventDate,
    TimeOnly StartTime,
    string VenueName,
    string CategoryName,
    decimal TicketPrice,
    decimal ParkingFee,
    string Reason
);

public record CustomerActivityItem(
    string ActivityType,
    int ReferenceId,
    string Title,
    string Description,
    decimal? Amount,
    DateTime OccurredAt
);

public record CustomerPaymentHistoryItem(
    int PaymentId,
    int BookingId,
    string BookingNumber,
    string EventName,
    decimal Amount,
    string PaymentMethod,
    string Status,
    string ReceiptNumber,
    DateTime PaidAt
);

public record CustomerUpcomingBookingItem(
    int BookingId,
    string BookingNumber,
    string EventName,
    DateOnly EventDate,
    TimeOnly StartTime,
    string VenueName,
    int SeatCount,
    string? ParkingSlot,
    decimal TotalAmount,
    string PaymentStatus
);
