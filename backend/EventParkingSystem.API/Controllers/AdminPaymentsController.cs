using EventParkingSystem.API.Common;
using EventParkingSystem.API.DTOs;
using EventParkingSystem.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventParkingSystem.API.Controllers;

[ApiController]
[Route("api/admin/payments")]
[Authorize(Roles = Roles.Admin)]
public sealed class AdminPaymentsController : ControllerBase
{
    private readonly IAdminService _service;
    public AdminPaymentsController(IAdminService service) => _service = service;

    [HttpGet]
    public async Task<ActionResult<List<AdminPaymentDto>>> Get() =>
        Ok(await _service.GetPaymentsAsync());
}
