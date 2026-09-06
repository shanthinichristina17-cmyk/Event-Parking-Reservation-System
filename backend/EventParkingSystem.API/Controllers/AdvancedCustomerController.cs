using EventParkingSystem.API.Common;
using EventParkingSystem.API.DTOs;
using EventParkingSystem.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventParkingSystem.API.Controllers;

[ApiController]
[Route("api/customer/advanced")]
[Authorize(Roles = Roles.Customer)]
public sealed class AdvancedCustomerController : ControllerBase
{
    private readonly IAdvancedCustomerService _service;

    public AdvancedCustomerController(IAdvancedCustomerService service) =>
        _service = service;

    [HttpGet("dashboard")]
    public async Task<ActionResult<CustomerAdvancedDashboardResponse>> Dashboard() =>
        Ok(await _service.DashboardAsync(User.CustomerId()));

    [HttpGet("spending")]
    public async Task<ActionResult<List<CustomerMonthlySpendingItem>>> Spending(
        [FromQuery] int months = 6) =>
        Ok(await _service.SpendingAsync(User.CustomerId(), months));

    [HttpGet("recommendations")]
    public async Task<ActionResult<List<RecommendedEventItem>>> Recommendations(
        [FromQuery] int limit = 6) =>
        Ok(await _service.RecommendationsAsync(User.CustomerId(), limit));

    [HttpGet("activity")]
    public async Task<ActionResult<List<CustomerActivityItem>>> Activity(
        [FromQuery] int limit = 20) =>
        Ok(await _service.ActivityAsync(User.CustomerId(), limit));

    [HttpGet("payments")]
    public async Task<ActionResult<List<CustomerPaymentHistoryItem>>> Payments() =>
        Ok(await _service.PaymentHistoryAsync(User.CustomerId()));

    [HttpGet("upcoming")]
    public async Task<ActionResult<List<CustomerUpcomingBookingItem>>> Upcoming(
        [FromQuery] int days = 30) =>
        Ok(await _service.UpcomingAsync(User.CustomerId(), days));

    [HttpGet("export/bookings.csv")]
    public async Task<IActionResult> ExportBookings()
    {
        var bytes = await _service.ExportBookingsCsvAsync(User.CustomerId());

        return File(
            bytes,
            "text/csv; charset=utf-8",
            $"my-bookings-{DateTime.UtcNow:yyyyMMdd-HHmmss}.csv");
    }

    [HttpGet("calendar.ics")]
    public async Task<IActionResult> Calendar()
    {
        var bytes = await _service.CalendarIcsAsync(User.CustomerId());

        return File(
            bytes,
            "text/calendar; charset=utf-8",
            "event-ease-bookings.ics");
    }
}
