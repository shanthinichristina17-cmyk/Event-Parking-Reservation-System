using System.Security.Cryptography;
using EventParkingSystem.API.Common;
using EventParkingSystem.API.DTOs;
using EventParkingSystem.API.Models;
using EventParkingSystem.API.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace EventParkingSystem.API.Services;

public interface IBookingService
{
    Task<BookingResponse> CreateAsync(int customerId, CreateBookingRequest request);
    Task<BookingResponse> GetAsync(int bookingId, int requesterId, bool isAdmin);
    Task<List<BookingResponse>> GetMineAsync(int customerId);
    Task<BookingResponse> CancelAsync(int bookingId, int requesterId, bool isAdmin);
    Task<int> ExpirePendingHoldsAsync();
}

public sealed class BookingService : IBookingService
{
    private readonly IBookingRepository _bookings;
    private readonly INotificationService _notifications;
    private readonly BookingSettings _settings;

    public BookingService(
        IBookingRepository bookings,
        INotificationService notifications,
        IOptions<BookingSettings> settings)
    {
        _bookings = bookings;
        _notifications = notifications;
        _settings = settings.Value;
    }

    public async Task<BookingResponse> CreateAsync(int customerId, CreateBookingRequest request)
    {
        if (request.SeatIds is null || request.SeatIds.Count == 0)
            throw ApiException.BadRequest("At least one seat is required.");

        var distinctSeatIds = request.SeatIds.Distinct().ToList();
        if (distinctSeatIds.Count != request.SeatIds.Count)
            throw ApiException.BadRequest("Duplicate seat IDs are not allowed.");

        await using var tx = await _bookings.BeginSerializableTransactionAsync();

        try
        {
            var customer = await _bookings.GetCustomerAsync(customerId)
                ?? throw ApiException.NotFound("Customer not found.");

            if (customer.Status != CustomerStatuses.Active)
                throw ApiException.Forbidden("Customer account is not active.");

            var eventEntity = await _bookings.GetEventAsync(request.EventId)
                ?? throw ApiException.NotFound("Event not found.");

            var seats = await _bookings.GetSeatsAsync(distinctSeatIds);

            if (seats.Count != distinctSeatIds.Count)
                throw ApiException.BadRequest("One or more selected seats do not exist.");

            if (seats.Any(x => x.EventId != request.EventId))
                throw ApiException.BadRequest("All selected seats must belong to the selected event.");

            var unavailableSeat = seats.FirstOrDefault(x => x.Status != SeatStatuses.Available);
            if (unavailableSeat is not null)
                throw ApiException.Conflict($"Seat {unavailableSeat.SeatRow}{unavailableSeat.SeatNumber} is not available.");

            ParkingSlot? slot = null;
            if (request.ParkingSlotId.HasValue)
            {
                slot = await _bookings.GetParkingSlotAsync(request.ParkingSlotId.Value)
                    ?? throw ApiException.NotFound("Parking slot not found.");

                if (slot.EventId != request.EventId)
                    throw ApiException.BadRequest("Parking slot must belong to the selected event.");

                if (slot.Status != ParkingStatuses.Available)
                    throw ApiException.Conflict("Selected parking slot is not available.");
            }

            var now = DateTime.UtcNow;
            var booking = new Booking
            {
                BookingNumber = GenerateBookingNumber(),
                CustomerId = customerId,
                EventId = request.EventId,
                Status = BookingStatuses.Pending,
                HoldExpiresAt = now.AddMinutes(Math.Max(1, _settings.HoldMinutes)),
                CreatedAt = now,
                UpdatedAt = now,
                TotalAmount = seats.Sum(x => x.Price) + (slot?.Fee ?? 0m)
            };

            foreach (var seat in seats)
            {
                seat.Status = SeatStatuses.Held;
                booking.BookingSeats.Add(new BookingSeat
                {
                    SeatId = seat.SeatId,
                    PriceAtBooking = seat.Price,
                    IsActive = true
                });
            }

            if (slot is not null)
            {
                slot.Status = ParkingStatuses.Held;
                booking.ParkingReservation = new ParkingReservation
                {
                    SlotId = slot.SlotId,
                    FeeAtReservation = slot.Fee,
                    IsActive = true
                };
            }

            await _bookings.AddAsync(booking);
            await _bookings.SaveChangesAsync();
            await tx.CommitAsync();

            var saved = await _bookings.GetByIdAsync(booking.BookingId)
                ?? throw ApiException.NotFound("Booking not found after creation.");

            return ToDto(saved);
        }
        catch (DbUpdateException)
        {
            await tx.RollbackAsync();
            throw ApiException.Conflict("One of the selected seats or the parking slot was taken by another booking.");
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }

    public async Task<BookingResponse> GetAsync(int bookingId, int requesterId, bool isAdmin)
    {
        var booking = await _bookings.GetByIdAsync(bookingId)
            ?? throw ApiException.NotFound("Booking not found.");

        EnsureOwnerOrAdmin(booking, requesterId, isAdmin);
        return ToDto(booking);
    }

    public async Task<List<BookingResponse>> GetMineAsync(int customerId) =>
        (await _bookings.GetForCustomerAsync(customerId)).Select(ToDto).ToList();

    public async Task<BookingResponse> CancelAsync(int bookingId, int requesterId, bool isAdmin)
    {
        await using var tx = await _bookings.BeginSerializableTransactionAsync();

        try
        {
            var booking = await _bookings.GetByIdAsync(bookingId)
                ?? throw ApiException.NotFound("Booking not found.");

            EnsureOwnerOrAdmin(booking, requesterId, isAdmin);

            if (booking.Status == BookingStatuses.Cancelled)
                return ToDto(booking);

            if (booking.Status == BookingStatuses.Expired)
                throw ApiException.Conflict("Expired booking cannot be cancelled.");

            foreach (var bookingSeat in booking.BookingSeats.Where(x => x.IsActive))
            {
                bookingSeat.IsActive = false;
                if (bookingSeat.Seat is not null)
                    bookingSeat.Seat.Status = SeatStatuses.Available;
            }

            if (booking.ParkingReservation is { IsActive: true } reservation)
            {
                reservation.IsActive = false;
                if (reservation.Slot is not null)
                    reservation.Slot.Status = ParkingStatuses.Available;
            }

            booking.Status = BookingStatuses.Cancelled;
            booking.HoldExpiresAt = null;
            booking.UpdatedAt = DateTime.UtcNow;

            await _bookings.SaveChangesAsync();
            await tx.CommitAsync();

            await _notifications.CreateAsync(
                booking.CustomerId,
                NotificationTypes.Cancellation,
                $"Booking {booking.BookingNumber} was cancelled.");

            return ToDto(booking);
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }

    public async Task<int> ExpirePendingHoldsAsync()
    {
        var expired = await _bookings.GetExpiredPendingAsync(DateTime.UtcNow);
        if (expired.Count == 0) return 0;

        foreach (var booking in expired)
        {
            foreach (var bookingSeat in booking.BookingSeats.Where(x => x.IsActive))
            {
                bookingSeat.IsActive = false;
                if (bookingSeat.Seat is not null)
                    bookingSeat.Seat.Status = SeatStatuses.Available;
            }

            if (booking.ParkingReservation is { IsActive: true } reservation)
            {
                reservation.IsActive = false;
                if (reservation.Slot is not null)
                    reservation.Slot.Status = ParkingStatuses.Available;
            }

            booking.Status = BookingStatuses.Expired;
            booking.HoldExpiresAt = null;
            booking.UpdatedAt = DateTime.UtcNow;
        }

        await _bookings.SaveChangesAsync();
        return expired.Count;
    }

    private static void EnsureOwnerOrAdmin(Booking booking, int requesterId, bool isAdmin)
    {
        if (!isAdmin && booking.CustomerId != requesterId)
            throw ApiException.Forbidden("You can only access your own bookings.");
    }

    private static string GenerateBookingNumber()
    {
        var suffix = RandomNumberGenerator.GetInt32(1000, 10000);
        return $"BK-{DateTime.UtcNow:yyyyMMddHHmmss}-{suffix}";
    }

    internal static BookingResponse ToDto(Booking booking)
    {
        var seatDtos = booking.BookingSeats
            .Where(x => x.IsActive && x.Seat is not null)
            .Select(x => new BookingSeatDto(
                x.SeatId,
                x.Seat!.SeatRow,
                x.Seat.SeatNumber,
                x.Seat.SeatType,
                x.PriceAtBooking))
            .ToList();

        ParkingSlotDto? parking = null;
        if (booking.ParkingReservation is { IsActive: true, Slot: not null } reservation)
        {
            var slot = reservation.Slot!;
            parking = new ParkingSlotDto(
                slot.SlotId,
                slot.EventId,
                slot.Zone,
                slot.SlotNumber,
                reservation.FeeAtReservation,
                slot.Status);
        }

        return new BookingResponse(
            booking.BookingId,
            booking.BookingNumber,
            booking.CustomerId,
            booking.EventId,
            booking.Event?.Name ?? string.Empty,
            booking.Status,
            booking.HoldExpiresAt,
            booking.TotalAmount,
            seatDtos,
            parking,
            booking.CreatedAt);
    }
}
