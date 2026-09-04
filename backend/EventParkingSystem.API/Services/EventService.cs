using EventParkingSystem.API.Common;
using EventParkingSystem.API.DTOs;
using EventParkingSystem.API.Models;
using EventParkingSystem.API.Repositories;

namespace EventParkingSystem.API.Services;

public interface IEventService
{
    Task<PagedResponse<EventResponse>> SearchAsync(EventSearchFilter filter);
    Task<EventResponse> GetByIdAsync(int id);
    Task<EventResponse> CreateAsync(CreateEventRequest request);
    Task<EventResponse> UpdateAsync(int id, UpdateEventRequest request);
    Task DeleteAsync(int id);
}

public sealed class EventService : IEventService
{
    private readonly IEventRepository _events;
    private readonly IVenueRepository _venues;
    private readonly ICategoryRepository _categories;
    private readonly INotificationService _notifications;
    private readonly ILogger<EventService> _logger;

    public EventService(IEventRepository events, IVenueRepository venues, ICategoryRepository categories, INotificationService notifications, ILogger<EventService> logger)
    {
        _events = events;
        _venues = venues;
        _categories = categories;
        _notifications = notifications;
        _logger = logger;
    }

    public async Task<PagedResponse<EventResponse>> SearchAsync(EventSearchFilter filter)
    {
        var page = Math.Max(1, filter.Page);
        var pageSize = Math.Clamp(filter.PageSize, 1, 100);
        var normalized = filter with { Page = page, PageSize = pageSize };
        var result = await _events.SearchAsync(normalized);
        return new PagedResponse<EventResponse>(result.Items.Select(Map).ToList(), page, pageSize, result.Total);
    }

    public async Task<EventResponse> GetByIdAsync(int id) =>
        Map(await _events.GetByIdAsync(id, tracking: false) ?? throw ApiException.NotFound("Event not found."));

    public async Task<EventResponse> CreateAsync(CreateEventRequest request)
    {
        await ValidateAsync(request.VenueId, request.CategoryId, request.EventDate, request.StartTime,
            request.EndTime, request.Capacity, excludeEventId: null);

        var ev = new Event
        {
            Name = request.Name.Trim(),
            VenueId = request.VenueId,
            CategoryId = request.CategoryId,
            EventDate = request.EventDate,
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            TicketPrice = request.TicketPrice,
            ParkingFee = request.ParkingFee,
            Capacity = request.Capacity
        };
        await _events.AddAsync(ev);
        await _events.SaveChangesAsync();
        return await GetByIdAsync(ev.EventId);
    }

    public async Task<EventResponse> UpdateAsync(int id, UpdateEventRequest request)
    {
        var ev = await _events.GetByIdAsync(id) ?? throw ApiException.NotFound("Event not found.");
        await ValidateAsync(request.VenueId, request.CategoryId, request.EventDate, request.StartTime,
            request.EndTime, request.Capacity, id);

        if (await _events.HasAnyBookingsAsync(id))
        {
            if (ev.TicketPrice != request.TicketPrice || ev.Capacity != request.Capacity || ev.VenueId != request.VenueId
                || ev.EventDate != request.EventDate || ev.StartTime != request.StartTime || ev.EndTime != request.EndTime)
            {
                throw ApiException.Conflict("Price, capacity, venue, date and time cannot be changed after bookings exist.");
            }
        }

        var customerIds = await _events.GetBookedCustomerIdsAsync(id);
        ev.Name = request.Name.Trim();
        ev.VenueId = request.VenueId;
        ev.CategoryId = request.CategoryId;
        ev.EventDate = request.EventDate;
        ev.StartTime = request.StartTime;
        ev.EndTime = request.EndTime;
        ev.TicketPrice = request.TicketPrice;
        ev.ParkingFee = request.ParkingFee;
        ev.Capacity = request.Capacity;
        ev.UpdatedAt = DateTime.UtcNow;
        await _events.SaveChangesAsync();

        foreach (var customerId in customerIds)
        {
            try
            {
                await _notifications.CreateAsync(customerId, NotificationTypes.Update,
                    $"Event '{ev.Name}' was updated. Please review your booking details.");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Event {EventId} was updated, but a notification for customer {CustomerId} could not be created.", id, customerId);
            }
        }

        return await GetByIdAsync(id);
    }

    public async Task DeleteAsync(int id)
    {
        var ev = await _events.GetByIdAsync(id) ?? throw ApiException.NotFound("Event not found.");
        if (await _events.HasAnyBookingsAsync(id))
            throw ApiException.Conflict("Event cannot be deleted while booking history exists.");
        _events.Remove(ev);
        await _events.SaveChangesAsync();
    }

    private async Task ValidateAsync(int venueId, int categoryId, DateOnly date, TimeOnly start, TimeOnly end,
        int capacity, int? excludeEventId)
    {
        if (date < DateOnly.FromDateTime(DateTime.UtcNow)) throw ApiException.BadRequest("Event date cannot be in the past.");
        if (end <= start) throw ApiException.BadRequest("End time must be after start time.");
        var venue = await _venues.GetByIdAsync(venueId) ?? throw ApiException.BadRequest("Selected venue does not exist.");
        _ = await _categories.GetByIdAsync(categoryId) ?? throw ApiException.BadRequest("Selected category does not exist.");
        if (capacity > venue.Capacity) throw ApiException.BadRequest("Event capacity cannot exceed venue capacity.");
        var conflict = await _venues.GetConflictingEventAsync(venueId, date, start, end, excludeEventId);
        if (conflict is not null) throw ApiException.Conflict($"Venue is already used by '{conflict.Name}' during this time.");
    }

    private static EventResponse Map(Event ev)
    {
        var booked = ev.Seats.Count(x => x.Status == SeatStatuses.Booked);
        var held = ev.Seats.Count(x => x.Status == SeatStatuses.Held);
        var available = ev.Seats.Count(x => x.Status == SeatStatuses.Available);
        return new EventResponse(ev.EventId, ev.Name, ev.VenueId, ev.Venue?.Name ?? string.Empty,
            ev.CategoryId, ev.Category?.Name ?? string.Empty, ev.EventDate, ev.StartTime, ev.EndTime,
            ev.TicketPrice, ev.ParkingFee, ev.Capacity, booked, held, available, ev.CreatedAt);
    }
}
