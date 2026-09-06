using System.Text;
using EventParkingSystem.API.Common;
using EventParkingSystem.API.DTOs;
using EventParkingSystem.API.Repositories;
using QRCoder;

namespace EventParkingSystem.API.Services;

public interface ITicketService
{
    Task<TicketResponse> GetTicketAsync(int bookingId, int requesterId, bool isAdmin);
    Task<byte[]> GenerateQrPngAsync(int bookingId, int requesterId, bool isAdmin);
}

public sealed class TicketService : ITicketService
{
    private readonly IBookingRepository _bookings;
    public TicketService(IBookingRepository bookings) => _bookings = bookings;

    public async Task<TicketResponse> GetTicketAsync(
        int bookingId,
        int requesterId,
        bool isAdmin)
    {
        var booking = await LoadConfirmedAsync(bookingId, requesterId, isAdmin);

        return new TicketResponse(
            booking.BookingId,
            booking.BookingNumber,
            booking.Event?.Name ?? string.Empty,
            booking.Event?.EventDate ?? default,
            booking.Event?.StartTime ?? default,
            booking.Event?.Venue?.Name ?? string.Empty,
            booking.BookingSeats
                .Where(x => x.IsActive && x.Seat is not null)
                .Select(x => $"{x.Seat!.SeatRow}{int.Parse(x.Seat.SeatNumber)}")
                .ToList(),
            booking.ParkingReservation is { IsActive: true, Slot: not null }
                ? booking.ParkingReservation.Slot!.SlotNumber
                : null,
            booking.Payment?.Amount ?? booking.TotalAmount,
            booking.Payment?.Status ?? PaymentStatuses.Pending);
    }

    public async Task<byte[]> GenerateQrPngAsync(
        int bookingId,
        int requesterId,
        bool isAdmin)
    {
        var ticket = await GetTicketAsync(bookingId, requesterId, isAdmin);

        var payload = new StringBuilder()
            .AppendLine("EVENT-EASE-TICKET")
            .AppendLine($"BOOKING={ticket.BookingNumber}")
            .AppendLine($"EVENT={ticket.EventName}")
            .AppendLine($"DATE={ticket.EventDate:yyyy-MM-dd}")
            .AppendLine($"TIME={ticket.StartTime:HH:mm}")
            .AppendLine($"VENUE={ticket.VenueName}")
            .AppendLine($"SEATS={string.Join(",", ticket.Seats)}")
            .AppendLine($"PARKING={ticket.ParkingSlot ?? "NONE"}")
            .AppendLine($"TOTAL={ticket.TotalPaid:0.00}")
            .ToString();

        using var generator = new QRCodeGenerator();
        using var data = generator.CreateQrCode(payload, QRCodeGenerator.ECCLevel.Q);
        var qr = new PngByteQRCode(data);
        return qr.GetGraphic(20);
    }

    private async Task<EventParkingSystem.API.Models.Booking> LoadConfirmedAsync(
        int bookingId,
        int requesterId,
        bool isAdmin)
    {
        var booking = await _bookings.GetByIdAsync(bookingId, tracking: false)
            ?? throw ApiException.NotFound("Booking not found.");

        if (!isAdmin && booking.CustomerId != requesterId)
            throw ApiException.Forbidden("You can only access your own ticket.");

        if (booking.Status != BookingStatuses.Confirmed ||
            booking.Payment?.Status != PaymentStatuses.Completed)
        {
            throw ApiException.Conflict(
                "Ticket is available only after successful payment.");
        }

        return booking;
    }
}
