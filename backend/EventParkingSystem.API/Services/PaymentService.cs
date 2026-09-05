using EventParkingSystem.API.Common;
using EventParkingSystem.API.DTOs;
using EventParkingSystem.API.Models;
using EventParkingSystem.API.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EventParkingSystem.API.Services;

public interface IPaymentService
{
    Task<PaymentResponse> PayAsync(int requesterId, bool isAdmin, CreatePaymentRequest request);
    Task<PaymentResponse> GetForBookingAsync(int bookingId, int requesterId, bool isAdmin);
}

public sealed class PaymentService : IPaymentService
{
    private readonly IBookingRepository _bookings;
    private readonly IPaymentRepository _payments;
    private readonly INotificationService _notifications;
    private readonly IEmailService _email;
    private readonly ILogger<PaymentService> _logger;

    public PaymentService(
        IBookingRepository bookings,
        IPaymentRepository payments,
        INotificationService notifications,
        IEmailService email,
        ILogger<PaymentService> logger)
    {
        _bookings = bookings;
        _payments = payments;
        _notifications = notifications;
        _email = email;
        _logger = logger;
    }

    public async Task<PaymentResponse> PayAsync(int requesterId, bool isAdmin, CreatePaymentRequest request)
    {
        await using var tx = await _bookings.BeginSerializableTransactionAsync();

        try
        {
            var booking = await _bookings.GetByIdAsync(request.BookingId)
                ?? throw ApiException.NotFound("Booking not found.");

            if (!isAdmin && booking.CustomerId != requesterId)
                throw ApiException.Forbidden("You can only pay for your own booking.");

            if (booking.Status != BookingStatuses.Pending)
                throw ApiException.Conflict("Only pending bookings can be paid.");

            if (booking.HoldExpiresAt.HasValue && booking.HoldExpiresAt <= DateTime.UtcNow)
                throw ApiException.Conflict("The booking hold has expired.");

            if (await _payments.GetByBookingIdAsync(booking.BookingId) is not null)
                throw ApiException.Conflict("This booking has already been paid.");

            var payment = new Payment
            {
                BookingId = booking.BookingId,
                Amount = booking.TotalAmount,
                Status = PaymentStatuses.Completed,
                PaidAt = DateTime.UtcNow,
                ReceiptNumber = $"RCPT-{DateTime.UtcNow:yyyyMMddHHmmss}-{booking.BookingId:D6}"
            };

            foreach (var bookingSeat in booking.BookingSeats.Where(x => x.IsActive))
            {
                if (bookingSeat.Seat is not null)
                    bookingSeat.Seat.Status = SeatStatuses.Booked;
            }

            if (booking.ParkingReservation is { IsActive: true, Slot: not null } reservation)
                reservation.Slot!.Status = ParkingStatuses.Reserved;

            booking.Status = BookingStatuses.Confirmed;
            booking.HoldExpiresAt = null;
            booking.UpdatedAt = DateTime.UtcNow;

            await _payments.AddAsync(payment);
            await _payments.SaveChangesAsync();
            await tx.CommitAsync();

            await _notifications.CreateAsync(
                booking.CustomerId,
                NotificationTypes.Payment,
                $"Payment completed for booking {booking.BookingNumber}. Receipt: {payment.ReceiptNumber}.");

            await _notifications.CreateAsync(
                booking.CustomerId,
                NotificationTypes.Confirmation,
                $"Booking {booking.BookingNumber} is confirmed.");

            if (booking.Customer is not null)
            {
                try
                {
                    await _email.SendBookingConfirmationAsync(
                        booking.Customer.Email,
                        booking.Customer.FullName,
                        booking.BookingNumber,
                        booking.TotalAmount);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Booking payment succeeded but confirmation email failed for booking {BookingId}.", booking.BookingId);
                }
            }

            return ToDto(payment);
        }
        catch (DbUpdateException)
        {
            await tx.RollbackAsync();
            throw ApiException.Conflict("Payment could not be completed because the booking changed. Please refresh and try again.");
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }

    public async Task<PaymentResponse> GetForBookingAsync(int bookingId, int requesterId, bool isAdmin)
    {
        var booking = await _bookings.GetByIdAsync(bookingId)
            ?? throw ApiException.NotFound("Booking not found.");

        if (!isAdmin && booking.CustomerId != requesterId)
            throw ApiException.Forbidden("You can only view your own payment.");

        var payment = await _payments.GetByBookingIdAsync(bookingId)
            ?? throw ApiException.NotFound("Payment not found.");

        return ToDto(payment);
    }

    private static PaymentResponse ToDto(Payment x) =>
        new(x.PaymentId, x.BookingId, x.Amount, x.Status, x.PaidAt, x.ReceiptNumber);
}
