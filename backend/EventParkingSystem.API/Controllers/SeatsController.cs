using EventParkingSystem.API.Common;
using EventParkingSystem.API.DTOs;
using EventParkingSystem.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventParkingSystem.API.Controllers;

[ApiController]
[Route("api/events/{eventId:int}/seats")]
public sealed class SeatsController : ControllerBase
{
    private readonly ISeatService _service;
    public SeatsController(ISeatService service) => _service = service;

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<List<SeatDto>>> Get(int eventId) =>
        Ok(await _service.GetForEventAsync(eventId));

    [HttpPost("generate")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<List<SeatDto>>> Generate(
        int eventId,
        [FromBody] GenerateSeatMapRequest request) =>
        Ok(await _service.GenerateAsync(eventId, request));

    [HttpDelete("{seatId:int}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Delete(int eventId, int seatId)
    {
        await _service.DeleteAsync(eventId, seatId);
        return NoContent();
    }
}
