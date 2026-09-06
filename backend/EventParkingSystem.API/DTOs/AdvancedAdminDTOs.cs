namespace EventParkingSystem.API.DTOs;

public record AnalyticsOverviewResponse(int TotalBookings,int ConfirmedBookings,int CancelledBookings,decimal ConfirmationRate,decimal CancellationRate,decimal GrossRevenue,decimal RefundAmount,decimal NetRevenue,decimal AverageConfirmedBookingValue,int TotalCustomers,int ActiveCustomers,int TotalEvents,int UpcomingEvents);
public record MonthlyRevenueItem(int Year,int Month,string Label,decimal Revenue,decimal Refunds,decimal NetRevenue);
public record TopEventAnalyticsItem(int EventId,string EventName,DateOnly EventDate,int ConfirmedBookings,int SeatsSold,int ParkingReservations,decimal Revenue);
public record CustomerGrowthItem(int Year,int Month,string Label,int NewCustomers,int CumulativeCustomers);
public record RecentActivityItem(string ActivityType,int ReferenceId,string ReferenceNumber,string Description,decimal? Amount,DateTime OccurredAt);
public record AdminSystemStatusResponse(string ApiStatus,bool DatabaseConnected,string Environment,DateTime UtcNow,int BookingHoldMinutes,int CancellationCutoffHours,int PendingBookings,int ExpiredBookings,int UnreadNotifications);
public record ReminderRunResponse(int EligibleBookings,int NotificationsCreated,int WithinHours,DateTime ExecutedAtUtc);
