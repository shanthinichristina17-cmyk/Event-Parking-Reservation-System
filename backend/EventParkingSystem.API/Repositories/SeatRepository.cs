using EventParkingSystem.API.Data;
using EventParkingSystem.API.Models;
using Microsoft.EntityFrameworkCore;

namespace EventParkingSystem.API.Repositories;

public interface ISeatRepository
{
    Task<Event?> GetEventAsync(int eventId);
    Task<List<Seat>> GetForEventAsync(int eventId, bool tracking = false);
    Task<Seat?> GetByIdAsync(int seatId);
    Task<bool> AnyForEventAsync(int eventId);
    Task AddRangeAsync(IEnumerable<Seat> seats);
    void Remove(Seat seat);
    Task<int> SaveChangesAsync();
}

public sealed class SeatRepository : ISeatRepository
{
    private readonly AppDbContext _db;
    public SeatRepository(AppDbContext db) => _db = db;

    public Task<Event?> GetEventAsync(int eventId) =>
        _db.Events.Include(x => x.Venue).FirstOrDefaultAsync(x => x.EventId == eventId);

    public Task<List<Seat>> GetForEventAsync(int eventId, bool tracking = false)
    {
        var query = _db.Seats.Where(x => x.EventId == eventId).AsQueryable();
        if (!tracking) query = query.AsNoTracking();
        return query.OrderBy(x => x.SeatRow).ThenBy(x => x.SeatNumber).ToListAsync();
    }

    public Task<Seat?> GetByIdAsync(int seatId) =>
        _db.Seats.FirstOrDefaultAsync(x => x.SeatId == seatId);

    public Task<bool> AnyForEventAsync(int eventId) =>
        _db.Seats.AnyAsync(x => x.EventId == eventId);

    public async Task AddRangeAsync(IEnumerable<Seat> seats) =>
        await _db.Seats.AddRangeAsync(seats);

    public void Remove(Seat seat) => _db.Seats.Remove(seat);

    public Task<int> SaveChangesAsync() => _db.SaveChangesAsync();
}
