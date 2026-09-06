using EventParkingSystem.API.Common;
using EventParkingSystem.API.DTOs;
using EventParkingSystem.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventParkingSystem.API.Controllers;

[ApiController]
[Route("api/dashboard")]
[Authorize]
public sealed class DashboardController : ControllerBase
{
    private readonly IAdminService _service;
    public DashboardController(IAdminService service) => _service = service;

    [HttpGet("admin")]
    [HttpGet("/api/admin/dashboard")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<AdminDashboardResponse>> AdminDashboard() =>
        Ok(await _service.GetDashboardAsync());

    [HttpGet("me")]
    [Authorize(Roles = Roles.Customer)]
    public async Task<ActionResult<CustomerDashboardResponse>> MyDashboard() =>
        Ok(await _service.GetCustomerDashboardAsync(User.CustomerId()));

    [HttpGet("customer/{customerId:int}")]
    public async Task<ActionResult<CustomerDashboardResponse>> CustomerDashboard(int customerId)
    {
        var requesterId = User.CustomerId();
        var isAdmin = User.IsInRole(Roles.Admin);

        if (!isAdmin && requesterId != customerId)
            throw ApiException.Forbidden("You can only view your own dashboard.");

        return Ok(await _service.GetCustomerDashboardAsync(customerId));
    }
}
