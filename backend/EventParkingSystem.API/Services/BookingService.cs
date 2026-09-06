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
    Task<BookingSummaryResponse> HoldSeatsAsync(int customerId, HoldSeatsRequest request);
    Task<BookingSummaryResponse> SelectParkingAsync(int bookingId, int requesterId, SelectParkingRequest request);
    Task<BookingSummaryResponse> ApplyPromoAsync(int bookingId, int requesterId, ApplyPromoRequest request);
    Task<BookingSummaryResponse> GetAsync(int bookingId, int requesterId, bool isAdmin);
    Task<List<BookingSummaryResponse>> GetMineAsync(int customerId, string? tab);
    Task<CancellationResponse> CancelAsync(int bookingId, int requesterId, bool isAdmin);
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

    public async Task<BookingSummaryResponse> HoldSeatsAsync(int customerId, HoldSeatsRequest request)
    {
        if (request.SeatIds is null || request.SeatIds.Count == 0)
            throw ApiException.BadRequest("Select at least one seat.");

        var ids = request.SeatIds.Distinct().ToList();
        if (ids.Count != request.SeatIds.Count)
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

            var eventStart = eventEntity.EventDate.ToDateTime(eventEntity.StartTime);
            if (eventStart <= DateTime.Now)
                throw ApiException.Conflict("This event has already started or finished.");

            var seats = await _bookings.GetSeatsAsync(ids);

            if (seats.Count != ids.Count)
                throw ApiException.BadRequest("One or more selected seats do not exist.");

            if (seats.Any(x => x.EventId != request.EventId))
                throw ApiException.BadRequest("All selected seats must belong to this event.");

            var unavailable = seats.FirstOrDefault(x => x.Status != SeatStatuses.Available);
            if (unavailable is not null)
                throw ApiException.Conflict(
                    $"Seat {unavailable.SeatRow}{unavailable.SeatNumber} is no longer available.");

            var now = DateTime.UtcNow;
            var subtotal = seats.Sum(x => x.Price);

            var booking = new Booking
            {
                BookingNumber = GenerateBookingNumber(),
                CustomerId = customerId,
                EventId = request.EventId,
                Status = BookingStatuses.Pending,
                HoldExpiresAt = now.AddMinutes(Math.Max(1, _settings.HoldMinutes)),
                TicketSubtotal = subtotal,
                ParkingFee = 0m,
                DiscountAmount = 0m,
                TotalAmount = subtotal,
                CreatedAt = now,
                UpdatedAt = now
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

            await _bookings.AddAsync(booking);
            await _bookings.SaveChangesAsync();
            await tx.CommitAsync();

            var saved = await _bookings.GetByIdAsync(booking.BookingId)
                ?? throw ApiException.NotFound("Booking hold not found after creation.");

            return ToDto(saved);
        }
        catch (DbUpdateException)
        {
            await tx.RollbackAsync();
            throw ApiException.Conflict(
                "One of the selected seats was taken by another customer. Please refresh the seat map.");
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }

    public async Task<BookingSummaryResponse> SelectParkingAsync(
        int bookingId,
        int requesterId,
        SelectParkingRequest request)
    {
        await using var tx = await _bookings.BeginSerializableTransactionAsync();

        try
        {
            var booking = await GetOwnedPendingBookingAsync(bookingId, requesterId);

            if (booking.ParkingReservation is { IsActive: true, Slot: not null } oldReservation)
            {
                oldReservation.Slot!.Status = ParkingStatuses.Available;
                oldReservation.IsActive = false;
                booking.ParkingFee = 0m;
            }

            if (request.ParkingSlotId.HasValue)
            {
                var slot = await _bookings.GetParkingSlotAsync(request.ParkingSlotId.Value)
                    ?? throw ApiException.NotFound("Parking slot not found.");

                if (slot.EventId != booking.EventId)
                    throw ApiException.BadRequest("Parking slot belongs to another event.");

                if (slot.IsDisabled || slot.Status == ParkingStatuses.Disabled)
                    throw ApiException.Conflict("Parking slot is disabled.");

                if (slot.Status != ParkingStatuses.Available)
                    throw ApiException.Conflict("Parking slot is no longer available.");

                slot.Status = ParkingStatuses.Held;

                if (booking.ParkingReservation is null)
                {
                    booking.ParkingReservation = new ParkingReservation
                    {
                        SlotId = slot.SlotId,
                        FeeAtReservation = slot.Fee,
                        IsActive = true
                    };
                }
                else
                {
                    booking.ParkingReservation.SlotId = slot.SlotId;
                    booking.ParkingReservation.Slot = slot;
                    booking.ParkingReservation.FeeAtReservation = slot.Fee;
                    booking.ParkingReservation.IsActive = true;
                }

                booking.ParkingFee = slot.Fee;
            }

            RecalculateTotal(booking);
            booking.UpdatedAt = DateTime.UtcNow;

            await _bookings.SaveChangesAsync();
            await tx.CommitAsync();

            var saved = await _bookings.GetByIdAsync(bookingId)
                ?? throw ApiException.NotFound("Booking not found.");

            return ToDto(saved);
        }
        catch (DbUpdateException)
        {
            await tx.RollbackAsync();
            throw ApiException.Conflict(
                "Parking slot was selected by another customer. Please choose another slot.");
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }

    public async Task<BookingSummaryResponse> ApplyPromoAsync(
        int bookingId,
        int requesterId,
        ApplyPromoRequest request)
    {
        var booking = await GetOwnedPendingBookingAsync(bookingId, requesterId);

        var code = request.PromoCode?.Trim().ToUpperInvariant();

        if (string.IsNullOrWhiteSpace(code))
        {
            booking.PromoCode = null;
            booking.DiscountAmount = 0m;
        }
        else if (code == "EVENT10")
        {
            booking.PromoCode = code;
            booking.DiscountAmount = Math.Round(booking.TicketSubtotal * 0.10m, 2);
        }
        else
        {
            throw ApiException.BadRequest("Invalid promo code.");
        }

        RecalculateTotal(booking);
        booking.UpdatedAt = DateTime.UtcNow;
        await _bookings.SaveChangesAsync();

        return ToDto(booking);
    }

    public async Task<BookingSummaryResponse> GetAsync(
        int bookingId,
        int requesterId,
        bool isAdmin)
    {
        var booking = await _bookings.GetByIdAsync(bookingId, tracking: false)
            ?? throw ApiException.NotFound("Booking not found.");

        EnsureOwnerOrAdmin(booking, requesterId, isAdmin);
        return ToDto(booking);
    }

    public async Task<List<BookingSummaryResponse>> GetMineAsync(int customerId, string? tab)
    {
        var bookings = await _bookings.GetForCustomerAsync(customerId);
        var now = DateTime.Now;
        var normalized = tab?.Trim().ToLowerInvariant();

        bookings = normalized switch
        {
            "cancelled" => bookings.Where(x => x.Status == BookingStatuses.Cancelled).ToList(),
            "past" => bookings.Where(x =>
                x.Event != null &&
                x.Event.EventDate.ToDateTime(x.Event.StartTime) < now &&
                x.Status != BookingStatuses.Cancelled).ToList(),
            "upcoming" => bookings.Where(x =>
                x.Event != null &&
                x.Event.EventDate.ToDateTime(x.Event.StartTime) >= now &&
                x.Status != BookingStatuses.Cancelled &&
                x.Status != BookingStatuses.Expired).ToList(),
            null or "" => bookings,
            _ => throw ApiException.BadRequest("tab must be upcoming, past or cancelled.")
        };

        return bookings.Select(ToDto).ToList();
    }

    public async Task<CancellationResponse> CancelAsync(
        int bookingId,
        int requesterId,
        bool isAdmin)
    {
        await using var tx = await _bookings.BeginSerializableTransactionAsync();

        try
        {
            var booking = await _bookings.GetByIdAsync(bookingId)
                ?? throw ApiException.NotFound("Booking not found.");

            EnsureOwnerOrAdmin(booking, requesterId, isAdmin);

            if (booking.Status == BookingStatuses.Cancelled)
            {
                return new CancellationResponse(
                    booking.BookingId,
                    booking.BookingNumber,
                    booking.Status,
                    booking.Refund is not null,
                    booking.Refund?.Amount ?? 0m,
                    booking.Refund?.Status,
                    "Booking is already cancelled.");
            }

            if (booking.Status == BookingStatuses.Expired)
                throw ApiException.Conflict("Expired booking cannot be cancelled.");

            if (booking.Event is null)
                throw ApiException.NotFound("Event not found for this booking.");

            var eventStart = booking.Event.EventDate.ToDateTime(booking.Event.StartTime);
            var cutoff = TimeSpan.FromHours(Math.Max(1, _settings.CancellationCutoffHours));

            if (eventStart - DateTime.Now <= cutoff)
                throw ApiException.Conflict(
                    $"Cancellation is not allowed within {_settings.CancellationCutoffHours} hours of the event.");

            ReleaseResources(booking);

            var refundSimulated = false;
            var refundAmount = 0m;
            string? refundStatus = null;

            if (booking.Payment?.Status == PaymentStatuses.Completed)
            {
                var refund = booking.Refund ?? new Refund
                {
                    BookingId = booking.BookingId,
                    PaymentId = booking.Payment.PaymentId,
                    Amount = booking.Payment.Amount,
                    Status = RefundStatuses.Simulated,
                    Reason = "Customer cancellation",
                    CreatedAt = DateTime.UtcNow
                };

                if (booking.Refund is null)
                    await _bookings.AddRefundAsync(refund);

                booking.Refund = refund;
                refundSimulated = true;
                refundAmount = refund.Amount;
                refundStatus = refund.Status;
            }

            booking.Status = BookingStatuses.Cancelled;
            booking.HoldExpiresAt = null;
            booking.UpdatedAt = DateTime.UtcNow;

            await _bookings.SaveChangesAsync();
            await tx.CommitAsync();

            await _notifications.CreateAsync(
                booking.CustomerId,
                NotificationTypes.Cancellation,
                refundSimulated
                    ? $"Booking {booking.BookingNumber} cancelled. Refund simulation: LKR {refundAmount:N2}."
                    : $"Booking {booking.BookingNumber} cancelled.");

            return new CancellationResponse(
                booking.BookingId,
                booking.BookingNumber,
                booking.Status,
                refundSimulated,
                refundAmount,
                refundStatus,
                "Booking cancelled and reserved resources released.");
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
            ReleaseResources(booking);
            booking.Status = BookingStatuses.Expired;
            booking.HoldExpiresAt = null;
            booking.UpdatedAt = DateTime.UtcNow;
        }

        await _bookings.SaveChangesAsync();
        return expired.Count;
    }

    private async Task<Booking> GetOwnedPendingBookingAsync(int bookingId, int requesterId)
    {
        var booking = await _bookings.GetByIdAsync(bookingId)
            ?? throw ApiException.NotFound("Booking not found.");

        if (booking.CustomerId != requesterId)
            throw ApiException.Forbidden("You can only update your own booking.");

        if (booking.Status != BookingStatuses.Pending)
            throw ApiException.Conflict("Only pending bookings can be changed.");

        if (!booking.HoldExpiresAt.HasValue || booking.HoldExpiresAt <= DateTime.UtcNow)
        {
            ReleaseResources(booking);
            booking.Status = BookingStatuses.Expired;
            booking.HoldExpiresAt = null;
            booking.UpdatedAt = DateTime.UtcNow;
            await _bookings.SaveChangesAsync();
            throw ApiException.Conflict("Seat hold expired. Please select seats again.");
        }

        return booking;
    }

    private static void RecalculateTotal(Booking booking)
    {
        booking.TotalAmount = Math.Max(
            0m,
            booking.TicketSubtotal + booking.ParkingFee - booking.DiscountAmount);
    }

    private static void ReleaseResources(Booking booking)
    {
        foreach (var item in booking.BookingSeats.Where(x => x.IsActive))
        {
            item.IsActive = false;
            if (item.Seat is not null)
                item.Seat.Status = SeatStatuses.Available;
        }

        if (booking.ParkingReservation is { IsActive: true } reservation)
        {
            reservation.IsActive = false;
            if (reservation.Slot is not null)
                reservation.Slot.Status = ParkingStatuses.Available;
        }
    }

    private static void EnsureOwnerOrAdmin(Booking booking, int requesterId, bool isAdmin)
    {
        if (!isAdmin && booking.CustomerId != requesterId)
            throw ApiException.Forbidden("You can only access your own booking.");
    }

    private static string GenerateBookingNumber()
    {
        var suffix = RandomNumberGenerator.GetInt32(1000, 10000);
        return $"BK-{DateTime.UtcNow:yyyyMMddHHmmss}-{suffix}";
    }

    internal static BookingSummaryResponse ToDto(Booking booking)
    {
        var seats = booking.BookingSeats
            .Where(x => x.IsActive && x.Seat is not null)
            .Select(x => new BookingSeatDto(
                x.SeatId,
                x.Seat!.SeatRow,
                x.Seat.SeatNumber,
                x.Seat.SeatType ?? SeatTypes.Regular,
                x.PriceAtBooking))
            .ToList();

        ParkingSlotDto? parking = null;
        if (booking.ParkingReservation is { IsActive: true, Slot: not null } reservation)
        {
            var slot = reservation.Slot!;
            parking = new ParkingSlotDto(
                slot.SlotId,
                slot.EventId,
                slot.Zone ?? string.Empty,
                slot.SlotNumber,
                slot.ParkingType ?? ParkingTypes.Normal,
                reservation.FeeAtReservation,
                slot.Status,
                slot.IsDisabled);
        }

        var holdSeconds = booking.HoldExpiresAt.HasValue
            ? Math.Max(0, (int)Math.Ceiling((booking.HoldExpiresAt.Value - DateTime.UtcNow).TotalSeconds))
            : 0;

        return new BookingSummaryResponse(
            booking.BookingId,
            booking.BookingNumber,
            booking.CustomerId,
            booking.EventId,
            booking.Event?.Name ?? string.Empty,
            booking.Event?.EventDate ?? default,
            booking.Event?.StartTime ?? default,
            booking.Event?.Venue?.Name ?? string.Empty,
            booking.Status,
            booking.HoldExpiresAt,
            holdSeconds,
            seats,
            parking,
            booking.TicketSubtotal,
            booking.ParkingFee,
            booking.PromoCode,
            booking.DiscountAmount,
            booking.TotalAmount,
            booking.Payment?.Status ?? PaymentStatuses.Pending,
            booking.Payment?.PaymentMethod,
            booking.Refund?.Status,
            booking.Refund?.Amount ?? 0m,
            booking.CreatedAt);
    }
}
