using EventParkingSystem.API.Data;
using EventParkingSystem.API.Models;
using Microsoft.EntityFrameworkCore;

namespace EventParkingSystem.API.Repositories;

public interface INotificationRepository
{
    Task AddAsync(Notification notification);
    Task<bool> ExistsAsync(int customerId, string type, string message);
    Task<List<Notification>> GetForCustomerAsync(int customerId);
    Task<Notification?> GetByIdAsync(int id);
    Task<int> SaveChangesAsync();
}

public sealed class NotificationRepository : INotificationRepository
{
    private readonly AppDbContext _db;
    public NotificationRepository(AppDbContext db) => _db = db;
    public async Task AddAsync(Notification notification) => await _db.Notifications.AddAsync(notification);
    public Task<bool> ExistsAsync(int customerId, string type, string message) =>
        _db.Notifications.AnyAsync(x => x.CustomerId == customerId && x.Type == type && x.Message == message);
    public Task<List<Notification>> GetForCustomerAsync(int customerId) => _db.Notifications.AsNoTracking()
        .Where(x => x.CustomerId == customerId).OrderByDescending(x => x.CreatedAt).ToListAsync();
    public Task<Notification?> GetByIdAsync(int id) => _db.Notifications.FirstOrDefaultAsync(x => x.NotificationId == id);
    public Task<int> SaveChangesAsync() => _db.SaveChangesAsync();
}
