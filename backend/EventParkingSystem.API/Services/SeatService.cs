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
    Task DeleteAsync(int eventId, int seatId);
}

public sealed class SeatService : ISeatService
{
    private readonly ISeatRepository _seats;

    public SeatService(ISeatRepository seats) => _seats = seats;

    public async Task<List<SeatDto>> GetForEventAsync(int eventId)
    {
        var eventEntity = await _seats.GetEventAsync(eventId)
            ?? throw ApiException.NotFound("Event not found.");

        return (await _seats.GetForEventAsync(eventEntity.EventId))
            .Select(ToDto).ToList();
    }

    public async Task<List<SeatDto>> GenerateAsync(int eventId, GenerateSeatMapRequest request)
    {
        if (request.Rows <= 0 || request.Rows > 200)
            throw ApiException.BadRequest("Rows must be between 1 and 200.");

        if (request.SeatsPerRow <= 0 || request.SeatsPerRow > 500)
            throw ApiException.BadRequest("SeatsPerRow must be between 1 and 500.");

        var eventEntity = await _seats.GetEventAsync(eventId)
            ?? throw ApiException.NotFound("Event not found.");

        var total = request.Rows * request.SeatsPerRow;
        if (total != eventEntity.Capacity)
            throw ApiException.BadRequest(
                $"Seat map must exactly match event capacity ({eventEntity.Capacity}). " +
                $"Requested map contains {total} seats.");

        if (await _seats.AnyForEventAsync(eventId))
            throw ApiException.Conflict("A seat map already exists for this event.");

        var price = request.Price ?? eventEntity.TicketPrice;
        if (price < 0) throw ApiException.BadRequest("Seat price cannot be negative.");

        var seatType = string.IsNullOrWhiteSpace(request.SeatType)
            ? "Standard"
            : request.SeatType.Trim();

        var entities = new List<Seat>(total);
        for (var row = 0; row < request.Rows; row++)
        {
            var rowLabel = ToRowLabel(row);
            for (var number = 1; number <= request.SeatsPerRow; number++)
            {
                entities.Add(new Seat
                {
                    EventId = eventId,
                    SeatRow = rowLabel,
                    SeatNumber = number.ToString("D3"),
                    SeatType = seatType,
                    Price = price,
                    Status = SeatStatuses.Available
                });
            }
        }

        try
        {
            await _seats.AddRangeAsync(entities);
            await _seats.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            throw ApiException.Conflict("Seat map could not be created because duplicate seats already exist.");
        }

        return entities.Select(ToDto).ToList();
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
        new(x.SeatId, x.EventId, x.SeatRow, x.SeatNumber, x.SeatType, x.Price, x.Status);

    private static string ToRowLabel(int index)
    {
        var value = index + 1;
        var label = string.Empty;
        while (value > 0)
        {
            value--;
            label = (char)('A' + (value % 26)) + label;
            value /= 26;
        }
        return label;
    }
}
