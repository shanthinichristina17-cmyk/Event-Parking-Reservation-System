using EventParkingSystem.API.Common;
using EventParkingSystem.API.DTOs;
using EventParkingSystem.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventParkingSystem.API.Controllers;

[ApiController]
[Route("api/admin/advanced")]
[Authorize(Roles=Roles.Admin)]
public sealed class AdvancedAdminController:ControllerBase
{
    readonly IAdvancedAdminService _service; readonly IEventReminderService _reminders;
    public AdvancedAdminController(IAdvancedAdminService service,IEventReminderService reminders){_service=service;_reminders=reminders;}

    [HttpGet("overview")] public async Task<ActionResult<AnalyticsOverviewResponse>> Overview()=>Ok(await _service.OverviewAsync());
    [HttpGet("monthly-revenue")] public async Task<ActionResult<List<MonthlyRevenueItem>>> MonthlyRevenue([FromQuery]int months=6)=>Ok(await _service.MonthlyRevenueAsync(months));
    [HttpGet("top-events")] public async Task<ActionResult<List<TopEventAnalyticsItem>>> TopEvents([FromQuery]int limit=5)=>Ok(await _service.TopEventsAsync(limit));
    [HttpGet("customer-growth")] public async Task<ActionResult<List<CustomerGrowthItem>>> CustomerGrowth([FromQuery]int months=6)=>Ok(await _service.CustomerGrowthAsync(months));
    [HttpGet("recent-activity")] public async Task<ActionResult<List<RecentActivityItem>>> RecentActivity([FromQuery]int limit=20)=>Ok(await _service.RecentActivityAsync(limit));
    [HttpGet("system-status")] public async Task<ActionResult<AdminSystemStatusResponse>> Status()=>Ok(await _service.SystemStatusAsync());
    [HttpPost("run-event-reminders")] public async Task<ActionResult<ReminderRunResponse>> RunReminders([FromQuery]int withinHours=24)=>Ok(await _reminders.RunAsync(withinHours));

    [HttpGet("export/bookings.csv")] public async Task<IActionResult> ExportBookings()=>File(await _service.ExportBookingsCsvAsync(),"text/csv; charset=utf-8",$"bookings-{DateTime.UtcNow:yyyyMMdd-HHmmss}.csv");
    [HttpGet("export/payments.csv")] public async Task<IActionResult> ExportPayments()=>File(await _service.ExportPaymentsCsvAsync(),"text/csv; charset=utf-8",$"payments-{DateTime.UtcNow:yyyyMMdd-HHmmss}.csv");
    [HttpGet("export/events.csv")] public async Task<IActionResult> ExportEvents()=>File(await _service.ExportEventsCsvAsync(),"text/csv; charset=utf-8",$"events-{DateTime.UtcNow:yyyyMMdd-HHmmss}.csv");
}
