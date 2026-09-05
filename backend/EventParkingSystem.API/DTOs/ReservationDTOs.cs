namespace EventParkingSystem.API.DTOs;

public record GenerateSeatMapRequest(
    int Rows,
    int SeatsPerRow,
    decimal? Price,
    string? SeatType
);

public record SeatDto(
    int SeatId,
    int EventId,
    string SeatRow,
    string SeatNumber,
    string? SeatType,
    decimal Price,
    string Status
);

public record GenerateParkingLayoutRequest(
    int SlotCount,
    string? Zone,
    decimal? Fee
);

public record ParkingSlotDto(
    int SlotId,
    int EventId,
    string? Zone,
    string SlotNumber,
    decimal Fee,
    string Status
);

public record CreateBookingRequest(
    int EventId,
    List<int> SeatIds,
    int? ParkingSlotId
);

public record BookingSeatDto(
    int SeatId,
    string SeatRow,
    string SeatNumber,
    string? SeatType,
    decimal Price
);

public record BookingResponse(
    int BookingId,
    string BookingNumber,
    int CustomerId,
    int EventId,
    string EventName,
    string Status,
    DateTime? HoldExpiresAt,
    decimal TotalAmount,
    List<BookingSeatDto> Seats,
    ParkingSlotDto? Parking,
    DateTime CreatedAt
);

public record CreatePaymentRequest(
    int BookingId
);

public record PaymentResponse(
    int PaymentId,
    int BookingId,
    decimal Amount,
    string Status,
    DateTime PaidAt,
    string ReceiptNumber
);
