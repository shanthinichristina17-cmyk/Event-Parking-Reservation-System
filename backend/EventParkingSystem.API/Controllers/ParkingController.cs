using EventParkingSystem.API.Common;
using EventParkingSystem.API.DTOs;
using EventParkingSystem.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventParkingSystem.API.Controllers;

[ApiController]
[Route("api/events/{eventId:int}/parking-slots")]
public sealed class ParkingController : ControllerBase
{
    private readonly IParkingService _service;
    public ParkingController(IParkingService service) => _service = service;

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<List<ParkingSlotDto>>> Get(int eventId) =>
        Ok(await _service.GetForEventAsync(eventId));

    [HttpPost("generate")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<List<ParkingSlotDto>>> Generate(
        int eventId,
        [FromBody] GenerateParkingLayoutRequest request) =>
        Ok(await _service.GenerateAsync(eventId, request));

    [HttpDelete("{slotId:int}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Delete(int eventId, int slotId)
    {
        await _service.DeleteAsync(eventId, slotId);
        return NoContent();
    }
}
