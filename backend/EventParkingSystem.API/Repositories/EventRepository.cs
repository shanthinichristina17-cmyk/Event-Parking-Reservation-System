using EventParkingSystem.API.Common;
using EventParkingSystem.API.Data;
using EventParkingSystem.API.DTOs;
using EventParkingSystem.API.Models;
using Microsoft.EntityFrameworkCore;

namespace EventParkingSystem.API.Repositories;

public interface IEventRepository
{
    Task<(List<Event> Items, int Total)> SearchAsync(EventSearchFilter filter);
    Task<Event?> GetByIdAsync(int id, bool tracking = true);
    Task<bool> HasAnyBookingsAsync(int eventId);
    Task<bool> HasActiveBookingsAsync(int eventId);
    Task<List<int>> GetBookedCustomerIdsAsync(int eventId);
    Task AddAsync(Event entity);
    void Remove(Event entity);
    Task<int> SaveChangesAsync();
}

public sealed class EventRepository : IEventRepository
{
    private readonly AppDbContext _db;
    public EventRepository(AppDbContext db) => _db = db;

    public async Task<(List<Event> Items, int Total)> SearchAsync(EventSearchFilter filter)
    {
        var query = _db.Events.AsNoTracking().Include(x => x.Venue).Include(x => x.Category).Include(x => x.Seats).AsQueryable();
        if (!string.IsNullOrWhiteSpace(filter.Name))
        {
            var term = filter.Name.Trim().ToLower();
            query = query.Where(x => x.Name.ToLower().Contains(term));
        }
        if (filter.Date.HasValue) query = query.Where(x => x.EventDate == filter.Date.Value);
        if (filter.VenueId.HasValue) query = query.Where(x => x.VenueId == filter.VenueId.Value);
        if (filter.CategoryId.HasValue) query = query.Where(x => x.CategoryId == filter.CategoryId.Value);

        var total = await query.CountAsync();
        var items = await query.OrderBy(x => x.EventDate).ThenBy(x => x.StartTime)
            .Skip((filter.Page - 1) * filter.PageSize).Take(filter.PageSize).ToListAsync();
        return (items, total);
    }

    public Task<Event?> GetByIdAsync(int id, bool tracking = true)
    {
        var query = _db.Events.Include(x => x.Venue).Include(x => x.Category).Include(x => x.Seats).AsQueryable();
        if (!tracking) query = query.AsNoTracking();
        return query.FirstOrDefaultAsync(x => x.EventId == id);
    }

    public Task<bool> HasAnyBookingsAsync(int eventId) => _db.Bookings.AnyAsync(x => x.EventId == eventId);
    public Task<bool> HasActiveBookingsAsync(int eventId) => _db.Bookings.AnyAsync(x => x.EventId == eventId
        && x.Status != BookingStatuses.Cancelled && x.Status != BookingStatuses.Expired);

    public Task<List<int>> GetBookedCustomerIdsAsync(int eventId) => _db.Bookings.AsNoTracking()
        .Where(x => x.EventId == eventId && x.Status != BookingStatuses.Cancelled && x.Status != BookingStatuses.Expired)
        .Select(x => x.CustomerId).Distinct().ToListAsync();

    public async Task AddAsync(Event entity) => await _db.Events.AddAsync(entity);
    public void Remove(Event entity) => _db.Events.Remove(entity);
    public Task<int> SaveChangesAsync() => _db.SaveChangesAsync();
}
