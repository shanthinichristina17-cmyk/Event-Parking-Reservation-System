using EventParkingSystem.API.Common;
using EventParkingSystem.API.DTOs;
using EventParkingSystem.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventParkingSystem.API.Controllers;

[ApiController]
[Route("api/admin/bookings")]
[Authorize(Roles = Roles.Admin)]
public sealed class AdminBookingsController : ControllerBase
{
    private readonly IAdminService _service;
    public AdminBookingsController(IAdminService service) => _service = service;

    [HttpGet]
    public async Task<ActionResult<List<AdminBookingDto>>> Get(
        [FromQuery] string? status,
        [FromQuery] string? search) =>
        Ok(await _service.GetBookingsAsync(status, search));
}
