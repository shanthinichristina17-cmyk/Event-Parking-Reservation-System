using System.Globalization;
using System.Text;
using EventParkingSystem.API.Common;
using EventParkingSystem.API.DTOs;
using EventParkingSystem.API.Models;
using EventParkingSystem.API.Repositories;

namespace EventParkingSystem.API.Services;

public interface IAdvancedCustomerService
{
    Task<CustomerAdvancedDashboardResponse> DashboardAsync(int customerId);
    Task<List<CustomerMonthlySpendingItem>> SpendingAsync(int customerId, int months);
    Task<List<RecommendedEventItem>> RecommendationsAsync(int customerId, int limit);
    Task<List<CustomerActivityItem>> ActivityAsync(int customerId, int limit);
    Task<List<CustomerPaymentHistoryItem>> PaymentHistoryAsync(int customerId);
    Task<List<CustomerUpcomingBookingItem>> UpcomingAsync(int customerId, int days);
    Task<byte[]> ExportBookingsCsvAsync(int customerId);
    Task<byte[]> CalendarIcsAsync(int customerId);
}

public sealed class AdvancedCustomerService : IAdvancedCustomerService
{
    private readonly IAdvancedCustomerRepository _repo;

    public AdvancedCustomerService(IAdvancedCustomerRepository repo) =>
        _repo = repo;

    public async Task<CustomerAdvancedDashboardResponse> DashboardAsync(int customerId)
    {
        await EnsureCustomerAsync(customerId);

        var bookings = await _repo.GetBookingsAsync(customerId);
        var notifications = await _repo.GetNotificationsAsync(customerId);

        var now = DateTime.Now;
        var confirmed = bookings.Count(x => x.Status == BookingStatuses.Confirmed);
        var pending = bookings.Count(x => x.Status == BookingStatuses.Pending);
        var cancelled = bookings.Count(x => x.Status == BookingStatuses.Cancelled);

        var upcomingRows = bookings
            .Where(x =>
                x.Status == BookingStatuses.Confirmed &&
                x.Event != null &&
                x.Event.EventDate.ToDateTime(x.Event.StartTime) >= now)
            .OrderBy(x => x.Event!.EventDate)
            .ThenBy(x => x.Event!.StartTime)
            .ToList();

        var totalSpent = bookings
            .Where(x => x.Payment?.Status == PaymentStatuses.Completed)
            .Sum(x => x.Payment!.Amount);

        var totalRefunded = bookings
            .Where(x => x.Refund is not null)
            .Sum(x => x.Refund!.Amount);

        var favoriteCategory = bookings
            .Where(x =>
                x.Status == BookingStatuses.Confirmed &&
                x.Event?.Category != null)
            .GroupBy(x => x.Event!.Category!.Name)
            .OrderByDescending(g => g.Count())
            .ThenBy(g => g.Key)
            .Select(g => g.Key)
            .FirstOrDefault() ?? "Not enough booking history";

        CustomerNextBookingItem? next = null;
        var nextBooking = upcomingRows.FirstOrDefault();

        if (nextBooking?.Event is not null)
        {
            next = new CustomerNextBookingItem(
                nextBooking.BookingId,
                nextBooking.BookingNumber,
                nextBooking.EventId,
                nextBooking.Event.Name,
                nextBooking.Event.EventDate,
                nextBooking.Event.StartTime,
                nextBooking.Event.Venue?.Name ?? string.Empty,
                nextBooking.BookingSeats.Count(x => x.IsActive),
                nextBooking.ParkingReservation is { IsActive: true, Slot: not null }
                    ? nextBooking.ParkingReservation.Slot!.SlotNumber
                    : null,
                nextBooking.TotalAmount);
        }

        return new CustomerAdvancedDashboardResponse(
            bookings.Count,
            confirmed,
            pending,
            cancelled,
            upcomingRows.Count,
            totalSpent,
            totalRefunded,
            bookings.Sum(x => x.DiscountAmount),
            bookings
                .Where(x => x.Status == BookingStatuses.Confirmed)
                .SelectMany(x => x.BookingSeats)
                .Count(x => x.IsActive),
            bookings.Count(x =>
                x.Status == BookingStatuses.Confirmed &&
                x.ParkingReservation is { IsActive: true }),
            notifications.Count(x => !x.IsRead),
            favoriteCategory,
            next);
    }

    public async Task<List<CustomerMonthlySpendingItem>> SpendingAsync(
        int customerId,
        int months)
    {
        await EnsureCustomerAsync(customerId);

        if (months is < 1 or > 24)
            throw ApiException.BadRequest("months must be between 1 and 24.");

        var bookings = await _repo.GetBookingsAsync(customerId);
        var start = new DateTime(
            DateTime.UtcNow.Year,
            DateTime.UtcNow.Month,
            1).AddMonths(-(months - 1));

        var result = new List<CustomerMonthlySpendingItem>();

        for (var i = 0; i < months; i++)
        {
            var date = start.AddMonths(i);

            var monthBookings = bookings.Where(x =>
                x.CreatedAt.Year == date.Year &&
                x.CreatedAt.Month == date.Month).ToList();

            var spent = monthBookings
                .Where(x => x.Payment?.Status == PaymentStatuses.Completed)
                .Sum(x => x.Payment!.Amount);

            var refunds = monthBookings
                .Where(x => x.Refund is not null)
                .Sum(x => x.Refund!.Amount);

            result.Add(new CustomerMonthlySpendingItem(
                date.Year,
                date.Month,
                date.ToString("MMM yyyy"),
                monthBookings.Count,
                spent,
                refunds,
                spent - refunds,
                monthBookings.Sum(x => x.DiscountAmount)));
        }

        return result;
    }

    public async Task<List<RecommendedEventItem>> RecommendationsAsync(
        int customerId,
        int limit)
    {
        await EnsureCustomerAsync(customerId);

        if (limit is < 1 or > 20)
            throw ApiException.BadRequest("limit must be between 1 and 20.");

        var bookings = await _repo.GetBookingsAsync(customerId);
        var today = DateOnly.FromDateTime(DateTime.Today);
        var events = await _repo.GetUpcomingEventsAsync(today);

        var bookedEventIds = bookings
            .Select(x => x.EventId)
            .ToHashSet();

        var preferredCategories = bookings
            .Where(x =>
                x.Status == BookingStatuses.Confirmed &&
                x.Event?.Category != null)
            .GroupBy(x => x.Event!.CategoryId)
            .OrderByDescending(g => g.Count())
            .Select(g => g.Key)
            .Take(3)
            .ToHashSet();

        var ranked = events
            .Where(x => !bookedEventIds.Contains(x.EventId))
            .Select(x => new
            {
                Event = x,
                Preferred = preferredCategories.Contains(x.CategoryId)
            })
            .OrderByDescending(x => x.Preferred)
            .ThenBy(x => x.Event.EventDate)
            .ThenBy(x => x.Event.StartTime)
            .Take(limit)
            .ToList();

        return ranked.Select(x => new RecommendedEventItem(
            x.Event.EventId,
            x.Event.Name,
            x.Event.EventDate,
            x.Event.StartTime,
            x.Event.Venue?.Name ?? string.Empty,
            x.Event.Category?.Name ?? string.Empty,
            x.Event.TicketPrice,
            x.Event.ParkingFee,
            x.Preferred
                ? "Recommended from your booking history"
                : "Upcoming event you have not booked yet"))
            .ToList();
    }

    public async Task<List<CustomerActivityItem>> ActivityAsync(
        int customerId,
        int limit)
    {
        await EnsureCustomerAsync(customerId);

        if (limit is < 1 or > 100)
            throw ApiException.BadRequest("limit must be between 1 and 100.");

        var bookings = await _repo.GetBookingsAsync(customerId);
        var payments = await _repo.GetPaymentsAsync(customerId);
        var refunds = await _repo.GetRefundsAsync(customerId);
        var notifications = await _repo.GetNotificationsAsync(customerId);

        return bookings.Select(x => new CustomerActivityItem(
                "Booking",
                x.BookingId,
                x.BookingNumber,
                $"{x.Event?.Name ?? "Event"} - {x.Status}",
                x.TotalAmount,
                x.CreatedAt))
            .Concat(payments.Select(x => new CustomerActivityItem(
                "Payment",
                x.PaymentId,
                x.ReceiptNumber,
                $"Payment {x.Status}",
                x.Amount,
                x.PaidAt)))
            .Concat(refunds.Select(x => new CustomerActivityItem(
                "Refund",
                x.RefundId,
                $"Refund REF-{x.RefundId}",
                x.Status,
                x.Amount,
                x.CreatedAt)))
            .Concat(notifications.Select(x => new CustomerActivityItem(
                "Notification",
                x.NotificationId,
                x.Type,
                x.Message,
                null,
                x.CreatedAt)))
            .OrderByDescending(x => x.OccurredAt)
            .Take(limit)
            .ToList();
    }

    public async Task<List<CustomerPaymentHistoryItem>> PaymentHistoryAsync(
        int customerId)
    {
        await EnsureCustomerAsync(customerId);

        var payments = await _repo.GetPaymentsAsync(customerId);

        return payments.Select(x => new CustomerPaymentHistoryItem(
            x.PaymentId,
            x.BookingId,
            x.Booking?.BookingNumber ?? string.Empty,
            x.Booking?.Event?.Name ?? string.Empty,
            x.Amount,
            x.PaymentMethod,
            x.Status,
            x.ReceiptNumber,
            x.PaidAt))
            .ToList();
    }

    public async Task<List<CustomerUpcomingBookingItem>> UpcomingAsync(
        int customerId,
        int days)
    {
        await EnsureCustomerAsync(customerId);

        if (days is < 1 or > 365)
            throw ApiException.BadRequest("days must be between 1 and 365.");

        var bookings = await _repo.GetBookingsAsync(customerId);
        var from = DateTime.Now;
        var to = from.AddDays(days);

        return bookings
            .Where(x =>
                x.Status == BookingStatuses.Confirmed &&
                x.Event != null)
            .Where(x =>
            {
                var eventStart = x.Event!.EventDate.ToDateTime(x.Event.StartTime);
                return eventStart >= from && eventStart <= to;
            })
            .OrderBy(x => x.Event!.EventDate)
            .ThenBy(x => x.Event!.StartTime)
            .Select(x => new CustomerUpcomingBookingItem(
                x.BookingId,
                x.BookingNumber,
                x.Event!.Name,
                x.Event.EventDate,
                x.Event.StartTime,
                x.Event.Venue?.Name ?? string.Empty,
                x.BookingSeats.Count(y => y.IsActive),
                x.ParkingReservation is { IsActive: true, Slot: not null }
                    ? x.ParkingReservation.Slot!.SlotNumber
                    : null,
                x.TotalAmount,
                x.Payment?.Status ?? PaymentStatuses.Pending))
            .ToList();
    }

    public async Task<byte[]> ExportBookingsCsvAsync(int customerId)
    {
        await EnsureCustomerAsync(customerId);

        var rows = await _repo.GetBookingsAsync(customerId);
        var sb = new StringBuilder();

        sb.AppendLine(
            "BookingId,BookingNumber,Event,Date,Time,Venue,Status,Seats,Parking,Subtotal,ParkingFee,Discount,Total,PaymentStatus,Refund,CreatedAt");

        foreach (var x in rows)
        {
            var seats = string.Join(" ",
                x.BookingSeats
                    .Where(y => y.IsActive && y.Seat != null)
                    .Select(y => $"{y.Seat!.SeatRow}{y.Seat.SeatNumber}"));

            sb.AppendLine(string.Join(",",
                x.BookingId,
                Csv(x.BookingNumber),
                Csv(x.Event?.Name),
                Csv(x.Event?.EventDate.ToString("yyyy-MM-dd")),
                Csv(x.Event?.StartTime.ToString("HH:mm")),
                Csv(x.Event?.Venue?.Name),
                Csv(x.Status),
                Csv(seats),
                Csv(x.ParkingReservation is { IsActive: true, Slot: not null }
                    ? x.ParkingReservation.Slot!.SlotNumber
                    : null),
                Money(x.TicketSubtotal),
                Money(x.ParkingFee),
                Money(x.DiscountAmount),
                Money(x.TotalAmount),
                Csv(x.Payment?.Status),
                Money(x.Refund?.Amount ?? 0m),
                Csv(x.CreatedAt.ToString("O"))));
        }

        return Utf8Bom(sb.ToString());
    }

    public async Task<byte[]> CalendarIcsAsync(int customerId)
    {
        await EnsureCustomerAsync(customerId);

        var rows = await _repo.GetBookingsAsync(customerId);
        var now = DateTime.Now;

        var upcoming = rows
            .Where(x =>
                x.Status == BookingStatuses.Confirmed &&
                x.Event != null &&
                x.Event.EventDate.ToDateTime(x.Event.StartTime) >= now)
            .OrderBy(x => x.Event!.EventDate)
            .ThenBy(x => x.Event!.StartTime)
            .ToList();

        var sb = new StringBuilder();
        sb.AppendLine("BEGIN:VCALENDAR");
        sb.AppendLine("VERSION:2.0");
        sb.AppendLine("PRODID:-//EventEase//CustomerBookings//EN");
        sb.AppendLine("CALSCALE:GREGORIAN");

        foreach (var booking in upcoming)
        {
            var ev = booking.Event!;
            var start = ev.EventDate.ToDateTime(ev.StartTime);
            var end = ev.EventDate.ToDateTime(ev.EndTime);

            sb.AppendLine("BEGIN:VEVENT");
            sb.AppendLine($"UID:{EscapeIcs(booking.BookingNumber)}@eventease.local");
            sb.AppendLine($"DTSTAMP:{DateTime.UtcNow:yyyyMMdd'T'HHmmss'Z'}");
            sb.AppendLine($"DTSTART:{start:yyyyMMdd'T'HHmmss}");
            sb.AppendLine($"DTEND:{end:yyyyMMdd'T'HHmmss}");
            sb.AppendLine($"SUMMARY:{EscapeIcs(ev.Name)}");
            sb.AppendLine($"LOCATION:{EscapeIcs(ev.Venue?.Name ?? string.Empty)}");
            sb.AppendLine(
                $"DESCRIPTION:{EscapeIcs($"Booking {booking.BookingNumber}. Seats: {booking.BookingSeats.Count(x => x.IsActive)}.")}");
            sb.AppendLine("END:VEVENT");
        }

        sb.AppendLine("END:VCALENDAR");

        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    private async Task EnsureCustomerAsync(int customerId)
    {
        if (await _repo.GetCustomerAsync(customerId) is null)
            throw ApiException.NotFound("Customer not found.");
    }

    private static string Money(decimal value) =>
        value.ToString("0.00", CultureInfo.InvariantCulture);

    private static string Csv(string? value) =>
        $"\"{(value ?? string.Empty).Replace("\"", "\"\"")}\"";

    private static byte[] Utf8Bom(string value) =>
        Encoding.UTF8.GetPreamble()
            .Concat(Encoding.UTF8.GetBytes(value))
            .ToArray();

    private static string EscapeIcs(string value) =>
        value.Replace("\\", "\\\\")
             .Replace(",", "\\,")
             .Replace(";", "\\;")
             .Replace("\r", string.Empty)
             .Replace("\n", "\\n");
}
