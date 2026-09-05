using EventParkingSystem.API.Data;
using EventParkingSystem.API.Models;
using Microsoft.EntityFrameworkCore;

namespace EventParkingSystem.API.Repositories;

public interface ISeatRepository
{
    Task<Event?> GetEventAsync(int eventId);
    Task<List<Seat>> GetForEventAsync(int eventId);
    Task<bool> AnyForEventAsync(int eventId);
    Task<Seat?> GetByIdAsync(int seatId);
    Task AddRangeAsync(IEnumerable<Seat> seats);
    void Remove(Seat seat);
    Task<int> SaveChangesAsync();
}

public sealed class SeatRepository : ISeatRepository
{
    private readonly AppDbContext _db;
    public SeatRepository(AppDbContext db) => _db = db;

    public Task<Event?> GetEventAsync(int eventId) =>
        _db.Events.FirstOrDefaultAsync(x => x.EventId == eventId);

    public Task<List<Seat>> GetForEventAsync(int eventId) =>
        _db.Seats.AsNoTracking()
            .Where(x => x.EventId == eventId)
            .OrderBy(x => x.SeatRow)
            .ThenBy(x => x.SeatNumber)
            .ToListAsync();

    public Task<bool> AnyForEventAsync(int eventId) =>
        _db.Seats.AnyAsync(x => x.EventId == eventId);

    public Task<Seat?> GetByIdAsync(int seatId) =>
        _db.Seats.FirstOrDefaultAsync(x => x.SeatId == seatId);

    public async Task AddRangeAsync(IEnumerable<Seat> seats) =>
        await _db.Seats.AddRangeAsync(seats);

    public void Remove(Seat seat) => _db.Seats.Remove(seat);

    public Task<int> SaveChangesAsync() => _db.SaveChangesAsync();
}
