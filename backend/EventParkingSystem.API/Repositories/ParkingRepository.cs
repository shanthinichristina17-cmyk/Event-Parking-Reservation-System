using EventParkingSystem.API.Data;
using EventParkingSystem.API.Models;
using Microsoft.EntityFrameworkCore;

namespace EventParkingSystem.API.Repositories;

public interface IParkingRepository
{
    Task<Event?> GetEventAsync(int eventId);
    Task<List<ParkingSlot>> GetForEventAsync(int eventId, bool tracking = false);
    Task<ParkingSlot?> GetByIdAsync(int slotId);
    Task AddRangeAsync(IEnumerable<ParkingSlot> slots);
    Task AddAsync(ParkingSlot slot);
    void Remove(ParkingSlot slot);
    Task<int> SaveChangesAsync();
}

public sealed class ParkingRepository : IParkingRepository
{
    private readonly AppDbContext _db;
    public ParkingRepository(AppDbContext db) => _db = db;

    public Task<Event?> GetEventAsync(int eventId) =>
        _db.Events.FirstOrDefaultAsync(x => x.EventId == eventId);

    public Task<List<ParkingSlot>> GetForEventAsync(int eventId, bool tracking = false)
    {
        var query = _db.ParkingSlots.Where(x => x.EventId == eventId).AsQueryable();
        if (!tracking) query = query.AsNoTracking();
        return query.OrderBy(x => x.Zone).ThenBy(x => x.SlotNumber).ToListAsync();
    }

    public Task<ParkingSlot?> GetByIdAsync(int slotId) =>
        _db.ParkingSlots.FirstOrDefaultAsync(x => x.SlotId == slotId);

    public async Task AddRangeAsync(IEnumerable<ParkingSlot> slots) =>
        await _db.ParkingSlots.AddRangeAsync(slots);

    public async Task AddAsync(ParkingSlot slot) => await _db.ParkingSlots.AddAsync(slot);

    public void Remove(ParkingSlot slot) => _db.ParkingSlots.Remove(slot);

    public Task<int> SaveChangesAsync() => _db.SaveChangesAsync();
}
