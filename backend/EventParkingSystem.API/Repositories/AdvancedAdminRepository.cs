using EventParkingSystem.API.Common;
using EventParkingSystem.API.Data;
using EventParkingSystem.API.Models;
using Microsoft.EntityFrameworkCore;

namespace EventParkingSystem.API.Repositories;

public interface IAdvancedAdminRepository
{
    Task<List<Booking>> GetBookingsAsync();
    Task<List<Payment>> GetPaymentsAsync();
    Task<List<Refund>> GetRefundsAsync();
    Task<List<Customer>> GetCustomersAsync();
    Task<List<Event>> GetEventsAsync();
    Task<List<Booking>> GetUpcomingConfirmedBookingsAsync(DateTime fromLocal, DateTime toLocal);
    Task<bool> CanConnectAsync();
    Task<int> CountUnreadNotificationsAsync();
    Task<bool> ReminderExistsAsync(int customerId, string message);
}

public sealed class AdvancedAdminRepository : IAdvancedAdminRepository
{
    private readonly AppDbContext _db;
    public AdvancedAdminRepository(AppDbContext db) => _db = db;

    public Task<List<Booking>> GetBookingsAsync() => _db.Bookings.AsNoTracking()
        .Include(x=>x.Customer).Include(x=>x.Event).ThenInclude(x=>x!.Venue)
        .Include(x=>x.BookingSeats).Include(x=>x.ParkingReservation).ThenInclude(x=>x!.Slot)
        .Include(x=>x.Payment).Include(x=>x.Refund).ToListAsync();

    public Task<List<Payment>> GetPaymentsAsync() => _db.Payments.AsNoTracking()
        .Include(x=>x.Booking).ThenInclude(x=>x!.Customer)
        .Include(x=>x.Booking).ThenInclude(x=>x!.Event).ToListAsync();

    public Task<List<Refund>> GetRefundsAsync() => _db.Refunds.AsNoTracking().ToListAsync();
    public Task<List<Customer>> GetCustomersAsync() => _db.Customers.AsNoTracking().Where(x=>x.Role==Roles.Customer).ToListAsync();
    public Task<List<Event>> GetEventsAsync() => _db.Events.AsNoTracking().Include(x=>x.Venue).Include(x=>x.Category).ToListAsync();

    public Task<List<Booking>> GetUpcomingConfirmedBookingsAsync(DateTime fromLocal, DateTime toLocal)
    {
        var fromDate=DateOnly.FromDateTime(fromLocal);
        var toDate=DateOnly.FromDateTime(toLocal);
        return _db.Bookings.AsNoTracking().Include(x=>x.Event).ThenInclude(x=>x!.Venue)
            .Where(x=>x.Status==BookingStatuses.Confirmed && x.Event!=null && x.Event.EventDate>=fromDate && x.Event.EventDate<=toDate)
            .ToListAsync();
    }

    public Task<bool> CanConnectAsync()=>_db.Database.CanConnectAsync();
    public Task<int> CountUnreadNotificationsAsync()=>_db.Notifications.CountAsync(x=>!x.IsRead);
    public Task<bool> ReminderExistsAsync(int customerId,string message)=>_db.Notifications.AnyAsync(x=>x.CustomerId==customerId&&x.Type==NotificationTypes.Reminder&&x.Message==message);
}
