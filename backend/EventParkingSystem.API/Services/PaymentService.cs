using EventParkingSystem.API.Common;
using EventParkingSystem.API.DTOs;
using EventParkingSystem.API.Models;
using EventParkingSystem.API.Repositories;

namespace EventParkingSystem.API.Services;

public interface IPaymentService
{
    Task<PaymentResponse> SimulateAsync(
        int bookingId,
        int requesterId,
        bool isAdmin,
        SimulatePaymentRequest request);

    Task<PaymentResponse> GetAsync(
        int bookingId,
        int requesterId,
        bool isAdmin);
}

public sealed class PaymentService : IPaymentService
{
    private readonly IBookingRepository _bookings;
    private readonly INotificationService _notifications;
    private readonly IEmailService _email;
    private readonly ILogger<PaymentService> _logger;

    public PaymentService(
        IBookingRepository bookings,
        INotificationService notifications,
        IEmailService email,
        ILogger<PaymentService> logger)
    {
        _bookings = bookings;
        _notifications = notifications;
        _email = email;
        _logger = logger;
    }

    public async Task<PaymentResponse> SimulateAsync(
        int bookingId,
        int requesterId,
        bool isAdmin,
        SimulatePaymentRequest request)
    {
        await using var tx = await _bookings.BeginSerializableTransactionAsync();

        try
        {
            var booking = await _bookings.GetByIdAsync(bookingId)
                ?? throw ApiException.NotFound("Booking not found.");

            if (!isAdmin && booking.CustomerId != requesterId)
                throw ApiException.Forbidden("You can only pay for your own booking.");

            if (booking.Status != BookingStatuses.Pending)
                throw ApiException.Conflict("Only pending bookings can be paid.");

            if (!booking.HoldExpiresAt.HasValue || booking.HoldExpiresAt <= DateTime.UtcNow)
                throw ApiException.Conflict("Seat hold expired. Please select seats again.");

            var method = string.IsNullOrWhiteSpace(request.PaymentMethod)
                ? "Card"
                : request.PaymentMethod.Trim();

            if (method.Length > 50)
                throw ApiException.BadRequest("PaymentMethod cannot exceed 50 characters.");

            ValidateSimulationFields(method, request);

            var maskedMethod = method;
            if (method.Equals("Card", StringComparison.OrdinalIgnoreCase) &&
                !string.IsNullOrWhiteSpace(request.CardNumber))
            {
                var digits = new string(request.CardNumber.Where(char.IsDigit).ToArray());
                var last4 = digits.Length >= 4 ? digits[^4..] : digits;
                maskedMethod = $"Card ****{last4}";
            }

            var payment = booking.Payment;
            if (payment?.Status == PaymentStatuses.Completed)
                throw ApiException.Conflict("This booking has already been paid.");

            if (payment is null)
            {
                payment = new Payment
                {
                    BookingId = booking.BookingId
                };
                await _bookings.AddPaymentAsync(payment);
                booking.Payment = payment;
            }

            payment.Amount = booking.TotalAmount;
            payment.PaymentMethod = maskedMethod;
            payment.PaidAt = DateTime.UtcNow;

            if (!request.Success)
            {
                payment.Status = PaymentStatuses.Failed;
                payment.FailureReason = "Simulated payment failure.";
                payment.ReceiptNumber = $"FAIL-{DateTime.UtcNow:yyyyMMddHHmmssfff}-{booking.BookingId}";

                await _bookings.SaveChangesAsync();
                await tx.CommitAsync();

                return ToDto(payment);
            }

            payment.Status = PaymentStatuses.Completed;
            payment.FailureReason = null;
            payment.ReceiptNumber = $"RCPT-{DateTime.UtcNow:yyyyMMddHHmmssfff}-{booking.BookingId}";

            foreach (var item in booking.BookingSeats.Where(x => x.IsActive))
            {
                if (item.Seat is not null)
                    item.Seat.Status = SeatStatuses.Booked;
            }

            if (booking.ParkingReservation is { IsActive: true, Slot: not null } reservation)
                reservation.Slot!.Status = ParkingStatuses.Booked;

            booking.Status = BookingStatuses.Confirmed;
            booking.HoldExpiresAt = null;
            booking.UpdatedAt = DateTime.UtcNow;

            await _bookings.SaveChangesAsync();
            await tx.CommitAsync();

            await _notifications.CreateAsync(
                booking.CustomerId,
                NotificationTypes.Payment,
                $"Payment successful for {booking.BookingNumber}. Receipt {payment.ReceiptNumber}.");

            await _notifications.CreateAsync(
                booking.CustomerId,
                NotificationTypes.Confirmation,
                $"Booking {booking.BookingNumber} confirmed.");

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
                    _logger.LogWarning(ex,
                        "Payment succeeded but confirmation email failed for booking {BookingId}.",
                        booking.BookingId);
                }
            }

            return ToDto(payment);
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }

    public async Task<PaymentResponse> GetAsync(
        int bookingId,
        int requesterId,
        bool isAdmin)
    {
        var booking = await _bookings.GetByIdAsync(bookingId, tracking: false)
            ?? throw ApiException.NotFound("Booking not found.");

        if (!isAdmin && booking.CustomerId != requesterId)
            throw ApiException.Forbidden("You can only view your own payment.");

        var payment = booking.Payment
            ?? throw ApiException.NotFound("Payment not found.");

        return ToDto(payment);
    }

    private static void ValidateSimulationFields(
        string method,
        SimulatePaymentRequest request)
    {
        if (!method.Equals("Card", StringComparison.OrdinalIgnoreCase))
            return;

        if (string.IsNullOrWhiteSpace(request.CardNumber) ||
            string.IsNullOrWhiteSpace(request.Expiry) ||
            string.IsNullOrWhiteSpace(request.Cvv))
        {
            throw ApiException.BadRequest(
                "Card number, expiry and CVV are required for card simulation.");
        }

        var digits = new string(request.CardNumber.Where(char.IsDigit).ToArray());
        if (digits.Length < 12 || digits.Length > 19)
            throw ApiException.BadRequest("Enter a valid simulated card number.");

        var cvv = new string(request.Cvv.Where(char.IsDigit).ToArray());
        if (cvv.Length is < 3 or > 4)
            throw ApiException.BadRequest("CVV must contain 3 or 4 digits.");

        // Full card number/CVV are never persisted.
    }

    private static PaymentResponse ToDto(Payment x) =>
        new(
            x.PaymentId,
            x.BookingId,
            x.Amount,
            x.PaymentMethod,
            x.Status,
            x.PaidAt,
            x.ReceiptNumber,
            x.FailureReason);
}
