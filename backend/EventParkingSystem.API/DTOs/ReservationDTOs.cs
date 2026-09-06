namespace EventParkingSystem.API.DTOs;

public record SeatDto(
    int SeatId,
    int EventId,
    string SeatRow,
    string SeatNumber,
    string SeatType,
    decimal Price,
    string Status
);

public record GenerateSeatMapRequest(
    int Rows,
    int Columns,
    decimal? RegularPrice,
    decimal? PremiumPrice,
    decimal? VipPrice
);

public record UpdateSeatRequest(
    string SeatType,
    decimal Price
);

public record ParkingSlotDto(
    int SlotId,
    int EventId,
    string Zone,
    string SlotNumber,
    string ParkingType,
    decimal Fee,
    string Status,
    bool IsDisabled
);

public record ParkingZoneRequest(
    string Zone,
    string ParkingType,
    int SlotCount,
    decimal Fee
);

public record GenerateParkingLayoutRequest(
    List<ParkingZoneRequest> Zones
);

public record CreateParkingSlotRequest(
    string Zone,
    string SlotNumber,
    string ParkingType,
    decimal Fee
);

public record UpdateParkingSlotRequest(
    string Zone,
    string ParkingType,
    decimal Fee,
    bool IsDisabled
);

public record HoldSeatsRequest(
    int EventId,
    List<int> SeatIds
);

public record SelectParkingRequest(
    int? ParkingSlotId
);

public record ApplyPromoRequest(
    string? PromoCode
);

public record BookingSeatDto(
    int SeatId,
    string SeatRow,
    string SeatNumber,
    string SeatType,
    decimal Price
);

public record BookingSummaryResponse(
    int BookingId,
    string BookingNumber,
    int CustomerId,
    int EventId,
    string EventName,
    DateOnly EventDate,
    TimeOnly StartTime,
    string VenueName,
    string Status,
    DateTime? HoldExpiresAt,
    int HoldSecondsRemaining,
    List<BookingSeatDto> Seats,
    ParkingSlotDto? Parking,
    decimal TicketSubtotal,
    decimal ParkingFee,
    string? PromoCode,
    decimal DiscountAmount,
    decimal FinalTotal,
    string PaymentStatus,
    string? PaymentMethod,
    string? RefundStatus,
    decimal RefundAmount,
    DateTime CreatedAt
);

public record SimulatePaymentRequest(
    bool Success,
    string PaymentMethod,
    string? CardNumber,
    string? Expiry,
    string? Cvv
);

public record PaymentResponse(
    int PaymentId,
    int BookingId,
    decimal Amount,
    string PaymentMethod,
    string Status,
    DateTime AttemptedAt,
    string ReceiptNumber,
    string? FailureReason
);

public record CancellationResponse(
    int BookingId,
    string BookingNumber,
    string BookingStatus,
    bool RefundSimulated,
    decimal RefundAmount,
    string? RefundStatus,
    string Message
);

public record TicketResponse(
    int BookingId,
    string BookingNumber,
    string EventName,
    DateOnly EventDate,
    TimeOnly StartTime,
    string VenueName,
    List<string> Seats,
    string? ParkingSlot,
    decimal TotalPaid,
    string PaymentStatus
);
