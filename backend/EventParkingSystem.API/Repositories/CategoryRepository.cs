using EventParkingSystem.API.Data;
using EventParkingSystem.API.Models;
using Microsoft.EntityFrameworkCore;

namespace EventParkingSystem.API.Repositories;

public interface ICategoryRepository
{
    Task<List<EventCategory>> GetAllAsync();
    Task<EventCategory?> GetByIdAsync(int id);
    Task<bool> NameExistsAsync(string normalizedName, int? excludeId = null);
    Task<bool> HasEventsAsync(int categoryId);
    Task AddAsync(EventCategory category);
    void Remove(EventCategory category);
    Task<int> SaveChangesAsync();
}

public sealed class CategoryRepository : ICategoryRepository
{
    private readonly AppDbContext _db;
    public CategoryRepository(AppDbContext db) => _db = db;
    public Task<List<EventCategory>> GetAllAsync() => _db.EventCategories.AsNoTracking().OrderBy(x => x.Name).ToListAsync();
    public Task<EventCategory?> GetByIdAsync(int id) => _db.EventCategories.FirstOrDefaultAsync(x => x.CategoryId == id);
    public Task<bool> NameExistsAsync(string normalizedName, int? excludeId = null) =>
        _db.EventCategories.AnyAsync(x => x.Name.ToLower() == normalizedName && (!excludeId.HasValue || x.CategoryId != excludeId.Value));
    public Task<bool> HasEventsAsync(int categoryId) => _db.Events.AnyAsync(x => x.CategoryId == categoryId);
    public async Task AddAsync(EventCategory category) => await _db.EventCategories.AddAsync(category);
    public void Remove(EventCategory category) => _db.EventCategories.Remove(category);
    public Task<int> SaveChangesAsync() => _db.SaveChangesAsync();
}
