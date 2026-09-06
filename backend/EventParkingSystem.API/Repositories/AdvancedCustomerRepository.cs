using EventParkingSystem.API.Common;
using EventParkingSystem.API.Data;
using EventParkingSystem.API.Models;
using Microsoft.EntityFrameworkCore;

namespace EventParkingSystem.API.Repositories;

public interface IAdvancedCustomerRepository
{
    Task<Customer?> GetCustomerAsync(int customerId);
    Task<List<Booking>> GetBookingsAsync(int customerId);
    Task<List<Payment>> GetPaymentsAsync(int customerId);
    Task<List<Refund>> GetRefundsAsync(int customerId);
    Task<List<Notification>> GetNotificationsAsync(int customerId);
    Task<List<Event>> GetUpcomingEventsAsync(DateOnly today);
}

public sealed class AdvancedCustomerRepository : IAdvancedCustomerRepository
{
    private readonly AppDbContext _db;

    public AdvancedCustomerRepository(AppDbContext db) => _db = db;

    public Task<Customer?> GetCustomerAsync(int customerId) =>
        _db.Customers.AsNoTracking()
            .FirstOrDefaultAsync(x =>
                x.CustomerId == customerId &&
                x.Role == Roles.Customer);

    public Task<List<Booking>> GetBookingsAsync(int customerId) =>
        _db.Bookings.AsNoTracking()
            .Include(x => x.Event)
                .ThenInclude(x => x!.Venue)
            .Include(x => x.Event)
                .ThenInclude(x => x!.Category)
            .Include(x => x.BookingSeats)
                .ThenInclude(x => x.Seat)
            .Include(x => x.ParkingReservation)
                .ThenInclude(x => x!.Slot)
            .Include(x => x.Payment)
            .Include(x => x.Refund)
            .Where(x => x.CustomerId == customerId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

    public Task<List<Payment>> GetPaymentsAsync(int customerId) =>
        _db.Payments.AsNoTracking()
            .Include(x => x.Booking)
                .ThenInclude(x => x!.Event)
            .Where(x =>
                x.Booking != null &&
                x.Booking.CustomerId == customerId)
            .OrderByDescending(x => x.PaidAt)
            .ToListAsync();

    public Task<List<Refund>> GetRefundsAsync(int customerId) =>
        _db.Refunds.AsNoTracking()
            .Include(x => x.Booking)
            .Where(x =>
                x.Booking != null &&
                x.Booking.CustomerId == customerId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

    public Task<List<Notification>> GetNotificationsAsync(int customerId) =>
        _db.Notifications.AsNoTracking()
            .Where(x => x.CustomerId == customerId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

    public Task<List<Event>> GetUpcomingEventsAsync(DateOnly today) =>
        _db.Events.AsNoTracking()
            .Include(x => x.Venue)
            .Include(x => x.Category)
            .Where(x => x.EventDate >= today)
            .OrderBy(x => x.EventDate)
            .ThenBy(x => x.StartTime)
            .ToListAsync();
}
