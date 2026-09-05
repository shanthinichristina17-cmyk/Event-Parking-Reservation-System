using EventParkingSystem.API.Common;
using EventParkingSystem.API.DTOs;
using EventParkingSystem.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventParkingSystem.API.Controllers;

[ApiController]
[Route("api/venues")]
public sealed class VenuesController : ControllerBase
{
    private readonly IVenueService _venues;
    public VenuesController(IVenueService venues) => _venues = venues;

    [HttpGet, AllowAnonymous]
    public async Task<IActionResult> GetAll() => Ok(await _venues.GetAllAsync());

    [HttpGet("{id:int}"), AllowAnonymous]
    public async Task<IActionResult> Get(int id) => Ok(await _venues.GetByIdAsync(id));

    [HttpGet("available"), AllowAnonymous]
    public async Task<IActionResult> Availability([FromQuery] DateOnly date, [FromQuery] TimeOnly startTime,
        [FromQuery] TimeOnly endTime, [FromQuery] int? venueId)
    {
        return venueId.HasValue
            ? Ok(await _venues.CheckAvailabilityAsync(venueId.Value, date, startTime, endTime))
            : Ok(await _venues.GetAllAvailabilityAsync(date, startTime, endTime));
    }

    [HttpPost, Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Create(CreateVenueRequest request)
    {
        var result = await _venues.CreateAsync(request);
        return CreatedAtAction(nameof(Get), new { id = result.VenueId }, result);
    }

    [HttpPut("{id:int}"), Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Update(int id, UpdateVenueRequest request) => Ok(await _venues.UpdateAsync(id, request));

    [HttpDelete("{id:int}"), Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Delete(int id)
    {
        await _venues.DeleteAsync(id);
        return NoContent();
    }
}
