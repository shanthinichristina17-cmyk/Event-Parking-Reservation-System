using EventParkingSystem.API.Common;
using EventParkingSystem.API.Data;
using EventParkingSystem.API.Models;
using Microsoft.EntityFrameworkCore;

namespace EventParkingSystem.API.Repositories;

public interface ICustomerRepository
{
    Task<Customer?> GetByIdAsync(int id);
    Task<Customer?> GetByEmailAsync(string normalizedEmail);
    Task<Customer?> GetByVerificationTokenAsync(string tokenHash);
    Task<Customer?> GetByResetTokenAsync(string tokenHash);
    Task<bool> EmailExistsAsync(string normalizedEmail, int? excludeCustomerId = null);
    Task AddAsync(Customer customer);
    Task<(List<Customer> Items, int Total)> SearchAsync(string? search, int page, int pageSize);
    Task<bool> HasActiveFutureBookingsAsync(int customerId);
    Task<(int Total, int Upcoming, int Cancelled)> GetBookingSummaryAsync(int customerId);
    Task<int> SaveChangesAsync();
}

public sealed class CustomerRepository : ICustomerRepository
{
    private readonly AppDbContext _db;
    public CustomerRepository(AppDbContext db) => _db = db;

    public Task<Customer?> GetByIdAsync(int id) => _db.Customers.FirstOrDefaultAsync(x => x.CustomerId == id);
    public Task<Customer?> GetByEmailAsync(string normalizedEmail) => _db.Customers.FirstOrDefaultAsync(x => x.Email == normalizedEmail);
    public Task<Customer?> GetByVerificationTokenAsync(string tokenHash) => _db.Customers.FirstOrDefaultAsync(x => x.EmailVerificationToken == tokenHash);
    public Task<Customer?> GetByResetTokenAsync(string tokenHash) => _db.Customers.FirstOrDefaultAsync(x => x.PasswordResetToken == tokenHash);

    public Task<bool> EmailExistsAsync(string normalizedEmail, int? excludeCustomerId = null) =>
        _db.Customers.AnyAsync(x => x.Email == normalizedEmail && (!excludeCustomerId.HasValue || x.CustomerId != excludeCustomerId.Value));

    public async Task AddAsync(Customer customer) => await _db.Customers.AddAsync(customer);

    public async Task<(List<Customer> Items, int Total)> SearchAsync(string? search, int page, int pageSize)
    {
        var query = _db.Customers.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(x => x.FullName.ToLower().Contains(term) || x.Email.ToLower().Contains(term));
        }

        var total = await query.CountAsync();
        var items = await query.OrderBy(x => x.FullName)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        return (items, total);
    }

    public Task<bool> HasActiveFutureBookingsAsync(int customerId)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        return _db.Bookings.AnyAsync(b => b.CustomerId == customerId
            && b.Event!.EventDate >= today
            && b.Status != BookingStatuses.Cancelled
            && b.Status != BookingStatuses.Expired);
    }

    public async Task<(int Total, int Upcoming, int Cancelled)> GetBookingSummaryAsync(int customerId)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var total = await _db.Bookings.CountAsync(b => b.CustomerId == customerId);
        var upcoming = await _db.Bookings.CountAsync(b => b.CustomerId == customerId && b.Event!.EventDate >= today
            && b.Status != BookingStatuses.Cancelled && b.Status != BookingStatuses.Expired);
        var cancelled = await _db.Bookings.CountAsync(b => b.CustomerId == customerId && b.Status == BookingStatuses.Cancelled);
        return (total, upcoming, cancelled);
    }

    public Task<int> SaveChangesAsync() => _db.SaveChangesAsync();
}
