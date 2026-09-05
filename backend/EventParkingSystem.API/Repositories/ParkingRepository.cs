using EventParkingSystem.API.Data;
using EventParkingSystem.API.Models;
using Microsoft.EntityFrameworkCore;

namespace EventParkingSystem.API.Repositories;

public interface IParkingRepository
{
    Task<Event?> GetEventAsync(int eventId);
    Task<List<ParkingSlot>> GetForEventAsync(int eventId);
    Task<bool> AnyForEventAsync(int eventId);
    Task<ParkingSlot?> GetByIdAsync(int slotId);
    Task AddRangeAsync(IEnumerable<ParkingSlot> slots);
    void Remove(ParkingSlot slot);
    Task<int> SaveChangesAsync();
}

public sealed class ParkingRepository : IParkingRepository
{
    private readonly AppDbContext _db;
    public ParkingRepository(AppDbContext db) => _db = db;

    public Task<Event?> GetEventAsync(int eventId) =>
        _db.Events.FirstOrDefaultAsync(x => x.EventId == eventId);

    public Task<List<ParkingSlot>> GetForEventAsync(int eventId) =>
        _db.ParkingSlots.AsNoTracking()
            .Where(x => x.EventId == eventId)
            .OrderBy(x => x.Zone)
            .ThenBy(x => x.SlotNumber)
            .ToListAsync();

    public Task<bool> AnyForEventAsync(int eventId) =>
        _db.ParkingSlots.AnyAsync(x => x.EventId == eventId);

    public Task<ParkingSlot?> GetByIdAsync(int slotId) =>
        _db.ParkingSlots.FirstOrDefaultAsync(x => x.SlotId == slotId);

    public async Task AddRangeAsync(IEnumerable<ParkingSlot> slots) =>
        await _db.ParkingSlots.AddRangeAsync(slots);

    public void Remove(ParkingSlot slot) => _db.ParkingSlots.Remove(slot);

    public Task<int> SaveChangesAsync() => _db.SaveChangesAsync();
}
