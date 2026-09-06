using EventParkingSystem.API.Common;
using EventParkingSystem.API.Data;
using EventParkingSystem.API.DTOs;
using EventParkingSystem.API.Models;
using Microsoft.EntityFrameworkCore;

namespace EventParkingSystem.API.Repositories;

public interface IAdminRepository
{
    Task<AdminDashboardResponse> GetDashboardAsync();
    Task<List<AdminBookingDto>> GetBookingsAsync(string? status, string? search);
    Task<List<AdminPaymentDto>> GetPaymentsAsync();
    Task<CustomerDashboardResponse> GetCustomerDashboardAsync(int customerId);
    Task<ReportSummaryResponse> GetReportSummaryAsync(DateTime? from, DateTime? to);
    Task<List<RevenueByEventDto>> GetRevenueByEventAsync(DateTime? from, DateTime? to);
    Task<List<BookingStatusDto>> GetBookingStatusBreakdownAsync(DateTime? from, DateTime? to);
    Task<int> GetUnreadNotificationCountAsync(int customerId);
    Task BroadcastNotificationAsync(string type, string message);
}

public sealed class AdminRepository : IAdminRepository
{
    private readonly AppDbContext _db;

    public AdminRepository(AppDbContext db) => _db = db;

    public async Task<AdminDashboardResponse> GetDashboardAsync()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var totalEvents = await _db.Events.CountAsync();
        var upcomingEvents = await _db.Events.CountAsync(x => x.EventDate >= today);

        var totalCustomers = await _db.Customers.CountAsync(x => x.Role == Roles.Customer);
        var activeCustomers = await _db.Customers.CountAsync(
            x => x.Role == Roles.Customer && x.Status == CustomerStatuses.Active);

        var totalBookings = await _db.Bookings.CountAsync();
        var pendingBookings = await _db.Bookings.CountAsync(x => x.Status == BookingStatuses.Pending);
        var confirmedBookings = await _db.Bookings.CountAsync(x => x.Status == BookingStatuses.Confirmed);
        var cancelledBookings = await _db.Bookings.CountAsync(x => x.Status == BookingStatuses.Cancelled);

        var totalRevenue = await _db.Payments
            .Where(x => x.Status == PaymentStatuses.Completed)
            .SumAsync(x => (decimal?)x.Amount) ?? 0m;

        var availableSeats = await _db.Seats.CountAsync(x => x.Status == SeatStatuses.Available);
        var heldSeats = await _db.Seats.CountAsync(x => x.Status == SeatStatuses.Held);
        var bookedSeats = await _db.Seats.CountAsync(x => x.Status == SeatStatuses.Booked);

        var availableParking = await _db.ParkingSlots.CountAsync(x => x.Status == ParkingStatuses.Available);
        var heldParking = await _db.ParkingSlots.CountAsync(x => x.Status == ParkingStatuses.Held);
        var reservedParking = await _db.ParkingSlots.CountAsync(x => x.Status == ParkingStatuses.Reserved);

        return new AdminDashboardResponse(
            totalEvents,
            upcomingEvents,
            totalCustomers,
            activeCustomers,
            totalBookings,
            pendingBookings,
            confirmedBookings,
            cancelledBookings,
            totalRevenue,
            availableSeats,
            heldSeats,
            bookedSeats,
            availableParking,
            heldParking,
            reservedParking);
    }

    public async Task<List<AdminBookingDto>> GetBookingsAsync(string? status, string? search)
    {
        var query = _db.Bookings.AsNoTracking()
            .Include(x => x.Customer)
            .Include(x => x.Event)
            .Include(x => x.BookingSeats)
            .Include(x => x.ParkingReservation)
                .ThenInclude(x => x!.Slot)
            .Include(x => x.Payment)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(x => x.Status == status.Trim());

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(x =>
                x.BookingNumber.ToLower().Contains(term) ||
                (x.Customer != null && x.Customer.FullName.ToLower().Contains(term)) ||
                (x.Customer != null && x.Customer.Email.ToLower().Contains(term)) ||
                (x.Event != null && x.Event.Name.ToLower().Contains(term)));
        }

        var items = await query
            .OrderByDescending(x => x.CreatedAt)
            .Take(500)
            .ToListAsync();

        return items.Select(MapBooking).ToList();
    }

    public async Task<List<AdminPaymentDto>> GetPaymentsAsync()
    {
        var items = await _db.Payments.AsNoTracking()
            .Include(x => x.Booking)
                .ThenInclude(x => x!.Customer)
            .Include(x => x.Booking)
                .ThenInclude(x => x!.Event)
            .OrderByDescending(x => x.PaidAt)
            .Take(500)
            .ToListAsync();

        return items.Select(x => new AdminPaymentDto(
            x.PaymentId,
            x.BookingId,
            x.Booking?.BookingNumber ?? string.Empty,
            x.Booking?.Customer?.FullName ?? string.Empty,
            x.Booking?.Customer?.Email ?? string.Empty,
            x.Booking?.Event?.Name ?? string.Empty,
            x.Amount,
            x.Status,
            x.ReceiptNumber,
            x.PaidAt)).ToList();
    }

    public async Task<CustomerDashboardResponse> GetCustomerDashboardAsync(int customerId)
    {
        var exists = await _db.Customers.AnyAsync(x => x.CustomerId == customerId);
        if (!exists) throw ApiException.NotFound("Customer not found.");

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var bookings = await _db.Bookings.AsNoTracking()
            .Include(x => x.Customer)
            .Include(x => x.Event)
            .Include(x => x.BookingSeats)
            .Include(x => x.ParkingReservation)
                .ThenInclude(x => x!.Slot)
            .Include(x => x.Payment)
            .Where(x => x.CustomerId == customerId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

        var totalSpent = bookings
            .Where(x => x.Payment != null && x.Payment.Status == PaymentStatuses.Completed)
            .Sum(x => x.Payment!.Amount);

        var upcoming = bookings.Count(x =>
            x.Event != null &&
            x.Event.EventDate >= today &&
            x.Status != BookingStatuses.Cancelled &&
            x.Status != BookingStatuses.Expired);

        var unread = await _db.Notifications.CountAsync(
            x => x.CustomerId == customerId && !x.IsRead);

        return new CustomerDashboardResponse(
            customerId,
            bookings.Count,
            bookings.Count(x => x.Status == BookingStatuses.Pending),
            bookings.Count(x => x.Status == BookingStatuses.Confirmed),
            upcoming,
            totalSpent,
            unread,
            bookings.Take(5).Select(MapBooking).ToList());
    }

    public async Task<ReportSummaryResponse> GetReportSummaryAsync(DateTime? from, DateTime? to)
    {
        var bookings = FilterBookingsByDate(from, to);
        var payments = FilterPaymentsByDate(from, to);

        var totalBookings = await bookings.CountAsync();
        var confirmedBookings = await bookings.CountAsync(x => x.Status == BookingStatuses.Confirmed);
        var cancelledBookings = await bookings.CountAsync(x => x.Status == BookingStatuses.Cancelled);
        var pendingBookings = await bookings.CountAsync(x => x.Status == BookingStatuses.Pending);

        var revenue = await payments
            .Where(x => x.Status == PaymentStatuses.Completed)
            .SumAsync(x => (decimal?)x.Amount) ?? 0m;

        var seatsSold = await bookings
            .Where(x => x.Status == BookingStatuses.Confirmed)
            .SelectMany(x => x.BookingSeats)
            .CountAsync(x => x.IsActive);

        var parkingReservations = await bookings
            .Where(x => x.Status == BookingStatuses.Confirmed && x.ParkingReservation != null)
            .CountAsync(x => x.ParkingReservation!.IsActive);

        return new ReportSummaryResponse(
            from,
            to,
            totalBookings,
            confirmedBookings,
            cancelledBookings,
            pendingBookings,
            revenue,
            confirmedBookings == 0 ? 0m : revenue / confirmedBookings,
            seatsSold,
            parkingReservations);
    }

    public async Task<List<RevenueByEventDto>> GetRevenueByEventAsync(DateTime? from, DateTime? to)
    {
        var payments = FilterPaymentsByDate(from, to)
            .Where(x => x.Status == PaymentStatuses.Completed);

        return await payments
            .Where(x => x.Booking != null && x.Booking.Event != null)
            .GroupBy(x => new
            {
                x.Booking!.EventId,
                EventName = x.Booking.Event!.Name
            })
            .Select(g => new RevenueByEventDto(
                g.Key.EventId,
                g.Key.EventName,
                g.Select(x => x.BookingId).Distinct().Count(),
                g.Sum(x => x.Amount)))
            .OrderByDescending(x => x.Revenue)
            .ToListAsync();
    }

    public async Task<List<BookingStatusDto>> GetBookingStatusBreakdownAsync(DateTime? from, DateTime? to)
    {
        return await FilterBookingsByDate(from, to)
            .GroupBy(x => x.Status)
            .Select(g => new BookingStatusDto(g.Key, g.Count()))
            .OrderByDescending(x => x.Count)
            .ToListAsync();
    }

    public Task<int> GetUnreadNotificationCountAsync(int customerId) =>
        _db.Notifications.CountAsync(x => x.CustomerId == customerId && !x.IsRead);

    public async Task BroadcastNotificationAsync(string type, string message)
    {
        var customerIds = await _db.Customers.AsNoTracking()
            .Where(x => x.Role == Roles.Customer && x.Status == CustomerStatuses.Active)
            .Select(x => x.CustomerId)
            .ToListAsync();

        var notifications = customerIds.Select(id => new Notification
        {
            CustomerId = id,
            Type = type,
            Message = message,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        });

        await _db.Notifications.AddRangeAsync(notifications);
        await _db.SaveChangesAsync();
    }

    private IQueryable<Booking> FilterBookingsByDate(DateTime? from, DateTime? to)
    {
        var query = _db.Bookings.AsNoTracking().AsQueryable();
        if (from.HasValue) query = query.Where(x => x.CreatedAt >= from.Value);
        if (to.HasValue) query = query.Where(x => x.CreatedAt <= to.Value);
        return query;
    }

    private IQueryable<Payment> FilterPaymentsByDate(DateTime? from, DateTime? to)
    {
        var query = _db.Payments.AsNoTracking()
            .Include(x => x.Booking)
                .ThenInclude(x => x!.Event)
            .AsQueryable();

        if (from.HasValue) query = query.Where(x => x.PaidAt >= from.Value);
        if (to.HasValue) query = query.Where(x => x.PaidAt <= to.Value);
        return query;
    }

    private static AdminBookingDto MapBooking(Booking x) =>
        new(
            x.BookingId,
            x.BookingNumber,
            x.CustomerId,
            x.Customer?.FullName ?? string.Empty,
            x.Customer?.Email ?? string.Empty,
            x.EventId,
            x.Event?.Name ?? string.Empty,
            x.Status,
            x.HoldExpiresAt,
            x.TotalAmount,
            x.BookingSeats.Count(y => y.IsActive),
            x.ParkingReservation is { IsActive: true, Slot: not null }
                ? x.ParkingReservation.Slot!.SlotNumber
                : null,
            x.Payment?.Status ?? "Not Paid",
            x.CreatedAt);
}
