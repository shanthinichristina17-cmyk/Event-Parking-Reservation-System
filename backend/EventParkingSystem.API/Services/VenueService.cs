using EventParkingSystem.API.Common;
using EventParkingSystem.API.DTOs;
using EventParkingSystem.API.Models;
using EventParkingSystem.API.Repositories;

namespace EventParkingSystem.API.Services;

public interface IVenueService
{
    Task<List<VenueResponse>> GetAllAsync();
    Task<VenueResponse> GetByIdAsync(int id);
    Task<VenueResponse> CreateAsync(CreateVenueRequest request);
    Task<VenueResponse> UpdateAsync(int id, UpdateVenueRequest request);
    Task DeleteAsync(int id);
    Task<VenueAvailabilityResponse> CheckAvailabilityAsync(int venueId, DateOnly date, TimeOnly start, TimeOnly end, int? excludeEventId = null);
    Task<List<VenueAvailabilityResponse>> GetAllAvailabilityAsync(DateOnly date, TimeOnly start, TimeOnly end);
}

public sealed class VenueService : IVenueService
{
    private readonly IVenueRepository _venues;
    public VenueService(IVenueRepository venues) => _venues = venues;

    public async Task<List<VenueResponse>> GetAllAsync() => (await _venues.GetAllAsync()).Select(Map).ToList();
    public async Task<VenueResponse> GetByIdAsync(int id) =>
        Map(await _venues.GetByIdAsync(id) ?? throw ApiException.NotFound("Venue not found."));

    public async Task<VenueResponse> CreateAsync(CreateVenueRequest request)
    {
        var venue = new Venue { Name = request.Name.Trim(), Address = request.Address.Trim(), Capacity = request.Capacity };
        await _venues.AddAsync(venue);
        await _venues.SaveChangesAsync();
        return Map(venue);
    }

    public async Task<VenueResponse> UpdateAsync(int id, UpdateVenueRequest request)
    {
        var venue = await _venues.GetByIdAsync(id) ?? throw ApiException.NotFound("Venue not found.");
        venue.Name = request.Name.Trim();
        venue.Address = request.Address.Trim();
        venue.Capacity = request.Capacity;
        venue.UpdatedAt = DateTime.UtcNow;
        await _venues.SaveChangesAsync();
        return Map(venue);
    }

    public async Task DeleteAsync(int id)
    {
        var venue = await _venues.GetByIdAsync(id) ?? throw ApiException.NotFound("Venue not found.");
        if (await _venues.HasAnyEventsAsync(id))
            throw ApiException.Conflict("Venue cannot be deleted while events are linked to it.");
        _venues.Remove(venue);
        await _venues.SaveChangesAsync();
    }

    public async Task<VenueAvailabilityResponse> CheckAvailabilityAsync(
        int venueId, DateOnly date, TimeOnly start, TimeOnly end, int? excludeEventId = null)
    {
        if (end <= start) throw ApiException.BadRequest("End time must be after start time.");
        var venue = await _venues.GetByIdAsync(venueId) ?? throw ApiException.NotFound("Venue not found.");
        var conflict = await _venues.GetConflictingEventAsync(venueId, date, start, end, excludeEventId);
        return new VenueAvailabilityResponse(venue.VenueId, venue.Name, conflict is null, conflict?.Name);
    }

    public async Task<List<VenueAvailabilityResponse>> GetAllAvailabilityAsync(DateOnly date, TimeOnly start, TimeOnly end)
    {
        if (end <= start) throw ApiException.BadRequest("End time must be after start time.");
        var result = new List<VenueAvailabilityResponse>();
        foreach (var venue in await _venues.GetAllAsync())
            result.Add(await CheckAvailabilityAsync(venue.VenueId, date, start, end));
        return result;
    }

    private static VenueResponse Map(Venue v) => new(v.VenueId, v.Name, v.Address, v.Capacity, v.CreatedAt);
}
