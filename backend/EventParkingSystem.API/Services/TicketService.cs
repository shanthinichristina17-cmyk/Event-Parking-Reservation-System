using System.Text;
using EventParkingSystem.API.Common;
using EventParkingSystem.API.Repositories;
using QRCoder;

namespace EventParkingSystem.API.Services;

public interface ITicketService
{
    Task<byte[]> GenerateQrPngAsync(int bookingId, int requesterId, bool isAdmin);
}

public sealed class TicketService : ITicketService
{
    private readonly IBookingRepository _bookings;

    public TicketService(IBookingRepository bookings) => _bookings = bookings;

    public async Task<byte[]> GenerateQrPngAsync(int bookingId, int requesterId, bool isAdmin)
    {
        var booking = await _bookings.GetByIdAsync(bookingId)
            ?? throw ApiException.NotFound("Booking not found.");

        if (!isAdmin && booking.CustomerId != requesterId)
            throw ApiException.Forbidden("You can only access your own ticket.");

        if (booking.Status != BookingStatuses.Confirmed)
            throw ApiException.Conflict("QR ticket is available only after successful payment.");

        var seats = string.Join(",",
            booking.BookingSeats
                .Where(x => x.IsActive && x.Seat is not null)
                .Select(x => $"{x.Seat!.SeatRow}{x.Seat.SeatNumber}"));

        var parking = booking.ParkingReservation is { IsActive: true, Slot: not null } p
            ? p.Slot!.SlotNumber
            : "NONE";

        var payload = new StringBuilder()
            .AppendLine("EVENT-PARK-TICKET")
            .AppendLine($"BOOKING={booking.BookingNumber}")
            .AppendLine($"EVENT={booking.Event?.Name}")
            .AppendLine($"DATE={booking.Event?.EventDate:yyyy-MM-dd}")
            .AppendLine($"SEATS={seats}")
            .AppendLine($"PARKING={parking}")
            .AppendLine($"TOTAL={booking.TotalAmount:0.00}")
            .ToString();

        using var generator = new QRCodeGenerator();
        using var data = generator.CreateQrCode(payload, QRCodeGenerator.ECCLevel.Q);
        var qr = new PngByteQRCode(data);
        return qr.GetGraphic(20);
    }
}
