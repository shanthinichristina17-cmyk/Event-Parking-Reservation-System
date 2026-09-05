using EventParkingSystem.API.Common;
using EventParkingSystem.API.DTOs;
using EventParkingSystem.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventParkingSystem.API.Controllers;

[ApiController]
[Route("api/customers")]
public sealed class CustomersController : ControllerBase
{
    private readonly ICustomerService _customers;
    private readonly IAuthService _auth;

    public CustomersController(ICustomerService customers, IAuthService auth)
    {
        _customers = customers;
        _auth = auth;
    }

    // BRD-compatible registration alias.
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register(RegisterCustomerRequest request)
        => StatusCode(StatusCodes.Status201Created, await _auth.RegisterAsync(request));

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> Me() => Ok(await _customers.GetAsync(User.CustomerId()));

    [HttpGet("{id:int}")]
    [Authorize]
    public async Task<IActionResult> Get(int id)
    {
        EnsureSelfOrAdmin(id);
        return Ok(await _customers.GetAsync(id));
    }

    [HttpPut("{id:int}")]
    [Authorize]
    public async Task<IActionResult> Update(int id, UpdateCustomerProfileRequest request)
    {
        EnsureSelfOrAdmin(id);
        return Ok(await _customers.UpdateAsync(id, request));
    }

    [HttpGet]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Search([FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        => Ok(await _customers.SearchAsync(search, page, pageSize));

    [HttpDelete("{id:int}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Deactivate(int id)
    {
        await _customers.DeactivateAsync(id);
        return NoContent();
    }

    [HttpPut("{id:int}/reactivate")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Reactivate(int id)
    {
        await _customers.ReactivateAsync(id);
        return NoContent();
    }

    private void EnsureSelfOrAdmin(int id)
    {
        if (!User.IsInRole(Roles.Admin) && User.CustomerId() != id)
            throw ApiException.Forbidden("You can only access your own customer profile.");
    }
}
