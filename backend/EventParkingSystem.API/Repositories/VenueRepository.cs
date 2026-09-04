using EventParkingSystem.API.Common;
using EventParkingSystem.API.Data;
using EventParkingSystem.API.Models;
using Microsoft.EntityFrameworkCore;

namespace EventParkingSystem.API.Repositories;

public interface IVenueRepository
{
    Task<List<Venue>> GetAllAsync();
    Task<Venue?> GetByIdAsync(int id);
    Task AddAsync(Venue venue);
    void Remove(Venue venue);
    Task<bool> HasUpcomingEventsAsync(int venueId);
    Task<bool> HasAnyEventsAsync(int venueId);
    Task<Event?> GetConflictingEventAsync(int venueId, DateOnly date, TimeOnly start, TimeOnly end, int? excludeEventId = null);
    Task<int> SaveChangesAsync();
}

public sealed class VenueRepository : IVenueRepository
{
    private readonly AppDbContext _db;
    public VenueRepository(AppDbContext db) => _db = db;
    public Task<List<Venue>> GetAllAsync() => _db.Venues.AsNoTracking().OrderBy(x => x.Name).ToListAsync();
    public Task<Venue?> GetByIdAsync(int id) => _db.Venues.FirstOrDefaultAsync(x => x.VenueId == id);
    public async Task AddAsync(Venue venue) => await _db.Venues.AddAsync(venue);
    public void Remove(Venue venue) => _db.Venues.Remove(venue);

    public Task<bool> HasAnyEventsAsync(int venueId) => _db.Events.AnyAsync(x => x.VenueId == venueId);

    public Task<bool> HasUpcomingEventsAsync(int venueId)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        return _db.Events.AnyAsync(x => x.VenueId == venueId && x.EventDate >= today);
    }

    public Task<Event?> GetConflictingEventAsync(int venueId, DateOnly date, TimeOnly start, TimeOnly end, int? excludeEventId = null) =>
        _db.Events.AsNoTracking().FirstOrDefaultAsync(x => x.VenueId == venueId && x.EventDate == date
            && x.StartTime < end && x.EndTime > start
            && (!excludeEventId.HasValue || x.EventId != excludeEventId.Value));

    public Task<int> SaveChangesAsync() => _db.SaveChangesAsync();
}
