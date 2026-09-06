using EventParkingSystem.API.Common;
using EventParkingSystem.API.DTOs;
using EventParkingSystem.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventParkingSystem.API.Controllers;

[ApiController]
[Route("api/customer/pro")]
[Authorize(Roles = Roles.Customer)]
public sealed class CustomerProInsightsController : ControllerBase
{
    private readonly IProInsightsService _service;

    public CustomerProInsightsController(IProInsightsService service) =>
        _service = service;

    [HttpGet("event-discovery")]
    public async Task<ActionResult<EventDiscoveryResponse>> Discover(
        [FromQuery] string? search = null,
        [FromQuery] int? categoryId = null,
        [FromQuery] int? venueId = null,
        [FromQuery] DateOnly? from = null,
        [FromQuery] DateOnly? to = null,
        [FromQuery] decimal? maxTicketPrice = null,
        [FromQuery] bool? parkingRequired = null,
        [FromQuery] string sort = "soonest",
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 12) =>
        Ok(await _service.DiscoverEventsAsync(
            search,
            categoryId,
            venueId,
            from,
            to,
            maxTicketPrice,
            parkingRequired,
            sort,
            page,
            pageSize));

    [HttpGet("events/{eventId:int}/availability")]
    public async Task<ActionResult<EventAvailabilityResponse>> Availability(int eventId) =>
        Ok(await _service.GetAvailabilityAsync(eventId));

    [HttpGet("events/{eventId:int}/price-estimate")]
    public async Task<ActionResult<BookingPriceEstimateResponse>> Estimate(
        int eventId,
        [FromQuery] int seatCount = 1,
        [FromQuery] bool includeParking = false) =>
        Ok(await _service.EstimateAsync(eventId, seatCount, includeParking));

    [HttpGet("booking-readiness")]
    public async Task<ActionResult<BookingReadinessResponse>> BookingReadiness() =>
        Ok(await _service.GetBookingReadinessAsync(User.CustomerId()));

    [HttpGet("savings")]
    public async Task<ActionResult<CustomerSavingsResponse>> Savings() =>
        Ok(await _service.GetSavingsAsync(User.CustomerId()));

    [HttpGet("events/{eventId:int}/similar")]
    public async Task<ActionResult<List<SimilarEventResponse>>> Similar(
        int eventId,
        [FromQuery] int limit = 4) =>
        Ok(await _service.GetSimilarEventsAsync(eventId, limit));
}
