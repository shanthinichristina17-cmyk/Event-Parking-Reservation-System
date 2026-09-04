using EventParkingSystem.API.Common;
using EventParkingSystem.API.DTOs;
using EventParkingSystem.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventParkingSystem.API.Controllers;

[ApiController]
[Route("api/events")]
public sealed class EventsController : ControllerBase
{
    private readonly IEventService _events;
    public EventsController(IEventService events) => _events = events;

    [HttpGet, AllowAnonymous]
    public async Task<IActionResult> Search([FromQuery] string? search, [FromQuery] string? name,
        [FromQuery] DateOnly? date, [FromQuery] int? venueId, [FromQuery] int? categoryId,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        => Ok(await _events.SearchAsync(new EventSearchFilter(search ?? name, date, venueId, categoryId, page, pageSize)));

    [HttpGet("{id:int}"), AllowAnonymous]
    public async Task<IActionResult> Get(int id) => Ok(await _events.GetByIdAsync(id));

    [HttpPost, Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Create(CreateEventRequest request)
    {
        var result = await _events.CreateAsync(request);
        return CreatedAtAction(nameof(Get), new { id = result.EventId }, result);
    }

    [HttpPut("{id:int}"), Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Update(int id, UpdateEventRequest request) => Ok(await _events.UpdateAsync(id, request));

    [HttpDelete("{id:int}"), Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Delete(int id)
    {
        await _events.DeleteAsync(id);
        return NoContent();
    }
}
