using EventParkingSystem.API.Common;
using EventParkingSystem.API.Data;
using EventParkingSystem.API.Models;
using Microsoft.EntityFrameworkCore;

namespace EventParkingSystem.API.Repositories;

public interface IProInsightsRepository
{
    Task<List<Event>> GetEventsDetailedAsync();
    Task<Event?> GetEventDetailedAsync(int eventId);
    Task<List<Booking>> GetCustomerBookingsAsync(int customerId);
    Task<int> GetUnreadNotificationsAsync(int customerId);
    Task<List<Payment>> GetPaymentsSinceAsync(DateTime fromUtc);
    Task<List<Booking>> GetAllBookingsDetailedAsync();
    Task<List<Customer>> GetCustomersAsync();
}

public sealed class ProInsightsRepository : IProInsightsRepository
{
    private readonly AppDbContext _db;

    public ProInsightsRepository(AppDbContext db) => _db = db;

    public Task<List<Event>> GetEventsDetailedAsync() =>
        _db.Events.AsNoTracking()
            .AsSplitQuery()
            .Include(x => x.Venue)
            .Include(x => x.Category)
            .Include(x => x.Seats)
            .Include(x => x.ParkingSlots)
            .Include(x => x.Bookings)
                .ThenInclude(x => x.Payment)
            .OrderBy(x => x.EventDate)
            .ThenBy(x => x.StartTime)
            .ToListAsync();

    public Task<Event?> GetEventDetailedAsync(int eventId) =>
        _db.Events.AsNoTracking()
            .AsSplitQuery()
            .Include(x => x.Venue)
            .Include(x => x.Category)
            .Include(x => x.Seats)
            .Include(x => x.ParkingSlots)
            .Include(x => x.Bookings)
                .ThenInclude(x => x.Payment)
            .FirstOrDefaultAsync(x => x.EventId == eventId);

    public Task<List<Booking>> GetCustomerBookingsAsync(int customerId) =>
        _db.Bookings.AsNoTracking()
            .Include(x => x.Event)
                .ThenInclude(x => x!.Venue)
            .Include(x => x.BookingSeats)
                .ThenInclude(x => x.Seat)
            .Include(x => x.ParkingReservation)
                .ThenInclude(x => x!.Slot)
            .Include(x => x.Payment)
            .Include(x => x.Refund)
            .Where(x => x.CustomerId == customerId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

    public Task<int> GetUnreadNotificationsAsync(int customerId) =>
        _db.Notifications.CountAsync(x =>
            x.CustomerId == customerId && !x.IsRead);

    public Task<List<Payment>> GetPaymentsSinceAsync(DateTime fromUtc) =>
        _db.Payments.AsNoTracking()
            .Include(x => x.Booking)
                .ThenInclude(x => x!.Event)
            .Where(x => x.PaidAt >= fromUtc)
            .OrderByDescending(x => x.PaidAt)
            .ToListAsync();

    public Task<List<Booking>> GetAllBookingsDetailedAsync() =>
        _db.Bookings.AsNoTracking()
            .Include(x => x.Customer)
            .Include(x => x.Event)
                .ThenInclude(x => x!.Venue)
            .Include(x => x.BookingSeats)
            .Include(x => x.ParkingReservation)
            .Include(x => x.Payment)
            .Include(x => x.Refund)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

    public Task<List<Customer>> GetCustomersAsync() =>
        _db.Customers.AsNoTracking()
            .Where(x => x.Role == Roles.Customer)
            .Include(x => x.Bookings)
                .ThenInclude(x => x.Payment)
            .ToListAsync();
}
