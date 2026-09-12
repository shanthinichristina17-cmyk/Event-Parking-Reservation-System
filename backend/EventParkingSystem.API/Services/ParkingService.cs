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
    Task<ParkingSlotDto> CreateAsync(int eventId, CreateParkingSlotRequest request);
    Task<ParkingSlotDto> UpdateAsync(int eventId, int slotId, UpdateParkingSlotRequest request);
    Task DeleteAsync(int eventId, int slotId);
}

public sealed class ParkingService : IParkingService
{
    private readonly IParkingRepository _parking;

    public ParkingService(IParkingRepository parking) => _parking = parking;

    public async Task<List<ParkingSlotDto>> GetForEventAsync(int eventId)
    {
        _ = await _parking.GetEventAsync(eventId)
            ?? throw ApiException.NotFound("Event not found.");

        return (await _parking.GetForEventAsync(eventId))
            .Select(ToDto).ToList();
    }

    public async Task<List<ParkingSlotDto>> GenerateAsync(int eventId, GenerateParkingLayoutRequest request)
    {
        _ = await _parking.GetEventAsync(eventId)
            ?? throw ApiException.NotFound("Event not found.");

        if (request.Zones is null || request.Zones.Count == 0)
            throw ApiException.BadRequest("At least one parking zone is required.");

        var existing = await _parking.GetForEventAsync(eventId);
        if (existing.Count > 0)
            throw ApiException.Conflict(
                "Parking layout already exists. Use add/edit/disable slot actions.");

        var slots = new List<ParkingSlot>();

        foreach (var zoneRequest in request.Zones)
        {
            if (zoneRequest.SlotCount <= 0)
                throw ApiException.BadRequest("Each zone SlotCount must be greater than zero.");

            if (zoneRequest.Fee < 0)
                throw ApiException.BadRequest("Parking fee cannot be negative.");

            var zone = zoneRequest.Zone.Trim().ToUpperInvariant();
            if (string.IsNullOrWhiteSpace(zone))
                throw ApiException.BadRequest("Zone is required.");

            var type = NormalizeParkingType(zoneRequest.ParkingType);

            for (var i = 1; i <= zoneRequest.SlotCount; i++)
            {
                slots.Add(new ParkingSlot
                {
                    EventId = eventId,
                    Zone = zone,
                    SlotNumber = $"{zone}-{i:D3}",
                    ParkingType = type,
                    Fee = zoneRequest.Fee,
                    Status = ParkingStatuses.Available,
                    IsDisabled = false
                });
            }
        }

        try
        {
            await _parking.AddRangeAsync(slots);
            await _parking.SaveChangesAsync();
            return slots.Select(ToDto).ToList();
        }
        catch (DbUpdateException)
        {
            throw ApiException.Conflict("Duplicate parking slot numbers detected.");
        }
    }

    public async Task<ParkingSlotDto> CreateAsync(int eventId, CreateParkingSlotRequest request)
    {
        _ = await _parking.GetEventAsync(eventId)
            ?? throw ApiException.NotFound("Event not found.");

        if (request.Fee < 0)
            throw ApiException.BadRequest("Parking fee cannot be negative.");

        if (string.IsNullOrWhiteSpace(request.Zone))
            throw ApiException.BadRequest("Zone is required.");

        if (string.IsNullOrWhiteSpace(request.SlotNumber))
            throw ApiException.BadRequest("SlotNumber is required.");

        var slot = new ParkingSlot
        {
            EventId = eventId,
            Zone = request.Zone.Trim().ToUpperInvariant(),
            SlotNumber = request.SlotNumber.Trim().ToUpperInvariant(),
            ParkingType = NormalizeParkingType(request.ParkingType),
            Fee = request.Fee,
            Status = ParkingStatuses.Available
        };

        await _parking.AddAsync(slot);

        try
        {
            await _parking.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            throw ApiException.Conflict("Parking slot number already exists for this event.");
        }

        return ToDto(slot);
    }

    public async Task<ParkingSlotDto> UpdateAsync(
        int eventId,
        int slotId,
        UpdateParkingSlotRequest request)
    {
        var slot = await _parking.GetByIdAsync(slotId)
            ?? throw ApiException.NotFound("Parking slot not found.");

        if (slot.EventId != eventId)
            throw ApiException.NotFound("Parking slot not found for this event.");

        if (slot.Status == ParkingStatuses.Held || slot.Status == ParkingStatuses.Booked)
            throw ApiException.Conflict("Held or booked parking slots cannot be edited.");

        if (request.Fee < 0)
            throw ApiException.BadRequest("Parking fee cannot be negative.");

        if (string.IsNullOrWhiteSpace(request.Zone))
            throw ApiException.BadRequest("Zone is required.");

        slot.Zone = request.Zone.Trim().ToUpperInvariant();
        slot.ParkingType = NormalizeParkingType(request.ParkingType);
        slot.Fee = request.Fee;
        slot.IsDisabled = request.IsDisabled;
        slot.Status = request.IsDisabled ? ParkingStatuses.Disabled : ParkingStatuses.Available;

        await _parking.SaveChangesAsync();
        return ToDto(slot);
    }

    public async Task DeleteAsync(int eventId, int slotId)
    {
        var slot = await _parking.GetByIdAsync(slotId)
            ?? throw ApiException.NotFound("Parking slot not found.");

        if (slot.EventId != eventId)
            throw ApiException.NotFound("Parking slot not found for this event.");

        if (slot.Status == ParkingStatuses.Held || slot.Status == ParkingStatuses.Booked)
            throw ApiException.Conflict("Held or booked parking slots cannot be deleted.");

        _parking.Remove(slot);
        await _parking.SaveChangesAsync();
    }

    private static ParkingSlotDto ToDto(ParkingSlot x) =>
        new(x.SlotId, x.EventId, x.Zone ?? string.Empty, x.SlotNumber,
            x.ParkingType ?? ParkingTypes.Normal, x.Fee, x.Status, x.IsDisabled);

    private static string NormalizeParkingType(string value)
    {
        var allowed = new[] { ParkingTypes.Vip, ParkingTypes.Standard, ParkingTypes.Normal };
        var trimmed = value.Trim();

        var match = allowed.FirstOrDefault(x => x.Equals(trimmed, StringComparison.OrdinalIgnoreCase));
        return match ?? throw ApiException.BadRequest("ParkingType must be VIP, Standard or Normal.");
    }
}
