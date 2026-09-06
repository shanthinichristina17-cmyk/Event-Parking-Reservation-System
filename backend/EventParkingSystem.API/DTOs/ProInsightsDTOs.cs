namespace EventParkingSystem.API.DTOs;

public record EventDiscoveryItemResponse(
    int EventId,
    string Name,
    int VenueId,
    string VenueName,
    int CategoryId,
    string CategoryName,
    DateOnly EventDate,
    TimeOnly StartTime,
    TimeOnly EndTime,
    decimal TicketPrice,
    decimal ParkingFee,
    int Capacity,
    int AvailableSeats,
    int HeldSeats,
    int BookedSeats,
    int AvailableParking,
    decimal? LowestAvailableSeatPrice,
    bool ParkingAvailable
);

public record EventDiscoveryResponse(
    int Page,
    int PageSize,
    int TotalItems,
    int TotalPages,
    List<EventDiscoveryItemResponse> Items
);

public record EventAvailabilityResponse(
    int EventId,
    string EventName,
    DateOnly EventDate,
    TimeOnly StartTime,
    int Capacity,
    int TotalSeats,
    int AvailableSeats,
    int HeldSeats,
    int BookedSeats,
    decimal SeatOccupancyPercent,
    decimal? LowestAvailableSeatPrice,
    decimal? HighestAvailableSeatPrice,
    int TotalParkingSlots,
    int AvailableParkingSlots,
    int HeldParkingSlots,
    int BookedParkingSlots,
    decimal ParkingOccupancyPercent,
    decimal? LowestAvailableParkingFee,
    bool CanBookSeats,
    bool CanBookParking
);

public record BookingPriceEstimateResponse(
    int EventId,
    string EventName,
    int RequestedSeatCount,
    bool IncludeParking,
    bool CanBook,
    decimal EstimatedSeatSubtotal,
    decimal EstimatedParkingFee,
    decimal EstimatedTotal,
    string Note
);

public record BookingReadinessResponse(
    int PendingBookings,
    int ActiveSeatHolds,
    DateTime? NextHoldExpiryUtc,
    int UpcomingConfirmedBookings,
    int UnreadNotifications,
    string Status,
    string Message
);

public record CustomerSavingsResponse(
    int PromoBookings,
    decimal TotalPromoSavings,
    decimal AveragePromoSaving,
    decimal TotalRefunded,
    decimal NetBenefit
);

public record SimilarEventResponse(
    int EventId,
    string Name,
    string VenueName,
    string CategoryName,
    DateOnly EventDate,
    TimeOnly StartTime,
    decimal TicketPrice,
    int AvailableSeats
);

public record AdminOperationsAlertResponse(
    string Severity,
    string Type,
    int? EventId,
    int? BookingId,
    string Title,
    string Message,
    DateTime DetectedAtUtc
);

public record EventCapacityUtilizationResponse(
    int EventId,
    string EventName,
    DateOnly EventDate,
    int SeatCapacity,
    int GeneratedSeats,
    int AvailableSeats,
    int HeldSeats,
    int BookedSeats,
    decimal SeatUtilizationPercent,
    int ParkingSlots,
    int AvailableParking,
    int HeldParking,
    int BookedParking,
    decimal ParkingUtilizationPercent
);

public record PaymentFailureReasonResponse(
    string Reason,
    int Count,
    decimal Amount
);

public record PaymentHealthResponse(
    int Days,
    int TotalPayments,
    int CompletedPayments,
    int FailedPayments,
    int PendingPayments,
    decimal SuccessRate,
    decimal CompletedAmount,
    decimal FailedAmount,
    List<PaymentFailureReasonResponse> FailureReasons
);

public record EventRevenueOpportunityItem(
    int EventId,
    string EventName,
    DateOnly EventDate,
    decimal RealizedRevenue,
    decimal AvailableSeatPotential,
    decimal AvailableParkingPotential,
    decimal RemainingPotential,
    decimal TotalRevenueOpportunity
);

public record CustomerSegmentsResponse(
    int TotalCustomers,
    int NewCustomersLast30Days,
    int ReturningCustomers,
    int HighValueCustomers,
    int CustomersWithUpcomingBookings,
    int DormantCustomers,
    string HighValueDefinition,
    string DormantDefinition
);

public record EventReadinessResponse(
    int EventId,
    string EventName,
    DateOnly EventDate,
    TimeOnly StartTime,
    int Capacity,
    int GeneratedSeats,
    int GeneratedParkingSlots,
    bool SeatMapReady,
    bool CapacityMatches,
    string ReadinessStatus,
    List<string> Issues
);
