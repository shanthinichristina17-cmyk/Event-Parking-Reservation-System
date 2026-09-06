using System.Data;
using EventParkingSystem.API.Common;
using EventParkingSystem.API.Data;
using EventParkingSystem.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace EventParkingSystem.API.Repositories;

public interface IBookingRepository
{
    Task<Customer?> GetCustomerAsync(int customerId);
    Task<Event?> GetEventAsync(int eventId);
    Task<List<Seat>> GetSeatsAsync(IEnumerable<int> seatIds);
    Task<ParkingSlot?> GetParkingSlotAsync(int slotId);
    Task<Booking?> GetByIdAsync(int bookingId, bool tracking = true);
    Task<List<Booking>> GetForCustomerAsync(int customerId);
    Task<List<Booking>> GetExpiredPendingAsync(DateTime utcNow);
    Task AddAsync(Booking booking);
    Task AddRefundAsync(Refund refund);
    Task AddPaymentAsync(Payment payment);
    void RemoveParkingReservation(ParkingReservation reservation);
    Task<IDbContextTransaction> BeginSerializableTransactionAsync();
    Task<int> SaveChangesAsync();
}

public sealed class BookingRepository : IBookingRepository
{
    private readonly AppDbContext _db;
    public BookingRepository(AppDbContext db) => _db = db;

    public Task<Customer?> GetCustomerAsync(int customerId) =>
        _db.Customers.FirstOrDefaultAsync(x => x.CustomerId == customerId);

    public Task<Event?> GetEventAsync(int eventId) =>
        _db.Events.Include(x => x.Venue).FirstOrDefaultAsync(x => x.EventId == eventId);

    public Task<List<Seat>> GetSeatsAsync(IEnumerable<int> seatIds)
    {
        var ids = seatIds.Distinct().ToList();
        return _db.Seats.Where(x => ids.Contains(x.SeatId)).ToListAsync();
    }

    public Task<ParkingSlot?> GetParkingSlotAsync(int slotId) =>
        _db.ParkingSlots.FirstOrDefaultAsync(x => x.SlotId == slotId);

    public Task<Booking?> GetByIdAsync(int bookingId, bool tracking = true)
    {
        var query = _db.Bookings
            .Include(x => x.Customer)
            .Include(x => x.Event)
                .ThenInclude(x => x!.Venue)
            .Include(x => x.BookingSeats)
                .ThenInclude(x => x.Seat)
            .Include(x => x.ParkingReservation)
                .ThenInclude(x => x!.Slot)
            .Include(x => x.Payment)
            .Include(x => x.Refund)
            .AsQueryable();

        if (!tracking) query = query.AsNoTracking();

        return query.FirstOrDefaultAsync(x => x.BookingId == bookingId);
    }

    public Task<List<Booking>> GetForCustomerAsync(int customerId) =>
        _db.Bookings.AsNoTracking()
            .Include(x => x.Event)
                .ThenInclude(x => x!.Venue)
            .Include(x => x.BookingSeats)
                .ThenInclude(x => x.Seat)
            .Include(x => x.ParkingReservation)
                .ThenInclude(x => x!.Slot)
            .Include(x => x.Payment)
            .Include(x => x.Refund)
            .Where(x => x.CustomerId == customerId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

    public Task<List<Booking>> GetExpiredPendingAsync(DateTime utcNow) =>
        _db.Bookings
            .Include(x => x.BookingSeats)
                .ThenInclude(x => x.Seat)
            .Include(x => x.ParkingReservation)
                .ThenInclude(x => x!.Slot)
            .Where(x => x.Status == BookingStatuses.Pending
                     && x.HoldExpiresAt != null
                     && x.HoldExpiresAt <= utcNow)
            .ToListAsync();

    public async Task AddAsync(Booking booking) => await _db.Bookings.AddAsync(booking);

    public async Task AddRefundAsync(Refund refund) => await _db.Refunds.AddAsync(refund);

    public async Task AddPaymentAsync(Payment payment) => await _db.Payments.AddAsync(payment);

    public void RemoveParkingReservation(ParkingReservation reservation) =>
        _db.ParkingReservations.Remove(reservation);

    public Task<IDbContextTransaction> BeginSerializableTransactionAsync() =>
        _db.Database.BeginTransactionAsync(IsolationLevel.Serializable);

    public Task<int> SaveChangesAsync() => _db.SaveChangesAsync();
}
