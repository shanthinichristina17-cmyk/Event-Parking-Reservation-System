using EventParkingSystem.API.Common;
using EventParkingSystem.API.DTOs;
using EventParkingSystem.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventParkingSystem.API.Controllers;

[ApiController]
[Route("api/settings")]
[Authorize(Roles = Roles.Admin)]
public sealed class SettingsController : ControllerBase
{
    private readonly IAdminSettingsService _settings;
    public SettingsController(IAdminSettingsService settings) => _settings = settings;

    [HttpGet]
    [HttpGet("/api/admin/settings")]
    public async Task<ActionResult<AdminSettingsResponse>> Get() =>
        Ok(await _settings.GetAsync());

    [HttpPut]
    [HttpPut("/api/admin/settings")]
    public async Task<ActionResult<AdminSettingsResponse>> Update(
        [FromBody] UpdateAdminSettingsRequest request) =>
        Ok(await _settings.UpdateAsync(request));
}
