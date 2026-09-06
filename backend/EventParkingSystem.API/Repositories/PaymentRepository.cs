using EventParkingSystem.API.Data;
using EventParkingSystem.API.Models;
using Microsoft.EntityFrameworkCore;

namespace EventParkingSystem.API.Repositories;

public interface IPaymentRepository
{
    Task<Payment?> GetByBookingIdAsync(int bookingId);
    Task AddAsync(Payment payment);
    Task<int> SaveChangesAsync();
}

public sealed class PaymentRepository : IPaymentRepository
{
    private readonly AppDbContext _db;
    public PaymentRepository(AppDbContext db) => _db = db;

    public Task<Payment?> GetByBookingIdAsync(int bookingId) =>
        _db.Payments.AsNoTracking().FirstOrDefaultAsync(x => x.BookingId == bookingId);

    public async Task AddAsync(Payment payment) => await _db.Payments.AddAsync(payment);

    public Task<int> SaveChangesAsync() => _db.SaveChangesAsync();
}
