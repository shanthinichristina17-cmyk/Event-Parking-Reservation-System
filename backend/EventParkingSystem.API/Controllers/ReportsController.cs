using EventParkingSystem.API.Common;
using EventParkingSystem.API.DTOs;
using EventParkingSystem.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventParkingSystem.API.Controllers;

[ApiController]
[Route("api/reports")]
[Authorize(Roles = Roles.Admin)]
public sealed class ReportsController : ControllerBase
{
    private readonly IAdminService _service;
    public ReportsController(IAdminService service) => _service = service;

    [HttpGet("summary")]
    [HttpGet("/api/admin/reports/summary")]
    public async Task<ActionResult<ReportSummaryResponse>> Summary(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to) =>
        Ok(await _service.GetReportSummaryAsync(from, to));

    [HttpGet("revenue-by-event")]
    [HttpGet("/api/admin/reports/revenue-by-event")]
    public async Task<ActionResult<List<RevenueByEventDto>>> RevenueByEvent(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to) =>
        Ok(await _service.GetRevenueByEventAsync(from, to));

    [HttpGet("booking-status")]
    [HttpGet("/api/admin/reports/booking-status")]
    public async Task<ActionResult<List<BookingStatusDto>>> BookingStatus(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to) =>
        Ok(await _service.GetBookingStatusBreakdownAsync(from, to));
}
