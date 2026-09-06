using EventParkingSystem.API.Common;
using EventParkingSystem.API.DTOs;
using EventParkingSystem.API.Models;
using EventParkingSystem.API.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EventParkingSystem.API.Services;

public interface ISeatService
{
    Task<List<SeatDto>> GetForEventAsync(int eventId);
    Task<List<SeatDto>> GenerateAsync(int eventId, GenerateSeatMapRequest request);
    Task<SeatDto> UpdateAsync(int eventId, int seatId, UpdateSeatRequest request);
    Task DeleteAsync(int eventId, int seatId);
}

public sealed class SeatService : ISeatService
{
    private readonly ISeatRepository _seats;

    public SeatService(ISeatRepository seats) => _seats = seats;

    public async Task<List<SeatDto>> GetForEventAsync(int eventId)
    {
        _ = await _seats.GetEventAsync(eventId)
            ?? throw ApiException.NotFound("Event not found.");

        return (await _seats.GetForEventAsync(eventId))
            .Select(ToDto).ToList();
    }

    public async Task<List<SeatDto>> GenerateAsync(int eventId, GenerateSeatMapRequest request)
    {
        if (request.Rows <= 0 || request.Columns <= 0)
            throw ApiException.BadRequest("Rows and columns must be greater than zero.");

        var eventEntity = await _seats.GetEventAsync(eventId)
            ?? throw ApiException.NotFound("Event not found.");

        var venueCapacity = eventEntity.Venue?.Capacity ?? eventEntity.Capacity;
        var total = checked(request.Rows * request.Columns);

        if (total > venueCapacity || total > eventEntity.Capacity)
            throw ApiException.BadRequest(
                $"Seat count ({total}) cannot exceed event/venue capacity.");

        if (await _seats.AnyForEventAsync(eventId))
            throw ApiException.Conflict(
                "A seat layout already exists. Edit individual seats or delete available seats before regenerating.");

        var regular = request.RegularPrice ?? eventEntity.TicketPrice;
        var premium = request.PremiumPrice ?? Math.Round(regular * 1.25m, 2);
        var vip = request.VipPrice ?? Math.Round(regular * 1.50m, 2);

        if (regular < 0 || premium < 0 || vip < 0)
            throw ApiException.BadRequest("Seat prices cannot be negative.");

        var seats = new List<Seat>(total);

        for (var row = 0; row < request.Rows; row++)
        {
            var label = RowLabel(row);

            for (var col = 1; col <= request.Columns; col++)
            {
                var ratio = (decimal)col / request.Columns;
                var type = ratio <= 0.20m ? SeatTypes.Vip :
                           ratio <= 0.50m ? SeatTypes.Premium :
                           SeatTypes.Regular;

                var price = type == SeatTypes.Vip ? vip :
                            type == SeatTypes.Premium ? premium :
                            regular;

                seats.Add(new Seat
                {
                    EventId = eventId,
                    SeatRow = label,
                    SeatNumber = col.ToString("D3"),
                    SeatType = type,
                    Price = price,
                    Status = SeatStatuses.Available
                });
            }
        }

        try
        {
            await _seats.AddRangeAsync(seats);
            await _seats.SaveChangesAsync();
            return seats.Select(ToDto).ToList();
        }
        catch (DbUpdateException)
        {
            throw ApiException.Conflict("Seat layout could not be generated due to duplicate/concurrent data.");
        }
    }

    public async Task<SeatDto> UpdateAsync(int eventId, int seatId, UpdateSeatRequest request)
    {
        var seat = await _seats.GetByIdAsync(seatId)
            ?? throw ApiException.NotFound("Seat not found.");

        if (seat.EventId != eventId)
            throw ApiException.NotFound("Seat not found for this event.");

        if (seat.Status != SeatStatuses.Available)
            throw ApiException.Conflict("Held or booked seats cannot be edited.");

        var allowed = new[] { SeatTypes.Vip, SeatTypes.Premium, SeatTypes.Regular };
        var type = request.SeatType.Trim();

        if (!allowed.Contains(type, StringComparer.OrdinalIgnoreCase))
            throw ApiException.BadRequest("SeatType must be VIP, Premium or Regular.");

        if (request.Price < 0)
            throw ApiException.BadRequest("Price cannot be negative.");

        seat.SeatType = allowed.First(x => x.Equals(type, StringComparison.OrdinalIgnoreCase));
        seat.Price = request.Price;
        await _seats.SaveChangesAsync();

        return ToDto(seat);
    }

    public async Task DeleteAsync(int eventId, int seatId)
    {
        var seat = await _seats.GetByIdAsync(seatId)
            ?? throw ApiException.NotFound("Seat not found.");

        if (seat.EventId != eventId)
            throw ApiException.NotFound("Seat not found for this event.");

        if (seat.Status != SeatStatuses.Available)
            throw ApiException.Conflict("Only available seats can be deleted.");

        _seats.Remove(seat);
        await _seats.SaveChangesAsync();
    }

    private static SeatDto ToDto(Seat x) =>
        new(x.SeatId, x.EventId, x.SeatRow, x.SeatNumber,
            x.SeatType ?? SeatTypes.Regular, x.Price, x.Status);

    private static string RowLabel(int index)
    {
        var value = index + 1;
        var label = string.Empty;

        while (value > 0)
        {
            value--;
            label = (char)('A' + value % 26) + label;
            value /= 26;
        }

        return label;
    }
}
