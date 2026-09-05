using EventParkingSystem.API.Common;
using EventParkingSystem.API.DTOs;
using EventParkingSystem.API.Models;
using EventParkingSystem.API.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EventParkingSystem.API.Services;

public interface IParkingService
{
    Task<List<ParkingSlotDto>> GetForEventAsync(int eventId);
    Task<List<ParkingSlotDto>> GenerateAsync(int eventId, GenerateParkingLayoutRequest request);
    Task DeleteAsync(int eventId, int slotId);
}

public sealed class ParkingService : IParkingService
{
    private readonly IParkingRepository _parking;

    public ParkingService(IParkingRepository parking) => _parking = parking;

    public async Task<List<ParkingSlotDto>> GetForEventAsync(int eventId)
    {
        var eventEntity = await _parking.GetEventAsync(eventId)
            ?? throw ApiException.NotFound("Event not found.");

        return (await _parking.GetForEventAsync(eventEntity.EventId))
            .Select(ToDto).ToList();
    }

    public async Task<List<ParkingSlotDto>> GenerateAsync(int eventId, GenerateParkingLayoutRequest request)
    {
        if (request.SlotCount <= 0 || request.SlotCount > 5000)
            throw ApiException.BadRequest("SlotCount must be between 1 and 5000.");

        var eventEntity = await _parking.GetEventAsync(eventId)
            ?? throw ApiException.NotFound("Event not found.");

        if (await _parking.AnyForEventAsync(eventId))
            throw ApiException.Conflict("A parking layout already exists for this event.");

        var zone = string.IsNullOrWhiteSpace(request.Zone) ? "A" : request.Zone.Trim();
        var fee = request.Fee ?? eventEntity.ParkingFee;
        if (fee < 0) throw ApiException.BadRequest("Parking fee cannot be negative.");

        var slots = Enumerable.Range(1, request.SlotCount)
            .Select(i => new ParkingSlot
            {
                EventId = eventId,
                Zone = zone,
                SlotNumber = $"{zone}-{i:D3}",
                Fee = fee,
                Status = ParkingStatuses.Available
            })
            .ToList();

        try
        {
            await _parking.AddRangeAsync(slots);
            await _parking.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            throw ApiException.Conflict("Parking layout could not be created because duplicate slots already exist.");
        }

        return slots.Select(ToDto).ToList();
    }

    public async Task DeleteAsync(int eventId, int slotId)
    {
        var slot = await _parking.GetByIdAsync(slotId)
            ?? throw ApiException.NotFound("Parking slot not found.");

        if (slot.EventId != eventId)
            throw ApiException.NotFound("Parking slot not found for this event.");

        if (slot.Status != ParkingStatuses.Available)
            throw ApiException.Conflict("Only available parking slots can be deleted.");

        _parking.Remove(slot);
        await _parking.SaveChangesAsync();
    }

    private static ParkingSlotDto ToDto(ParkingSlot x) =>
        new(x.SlotId, x.EventId, x.Zone, x.SlotNumber, x.Fee, x.Status);
}
