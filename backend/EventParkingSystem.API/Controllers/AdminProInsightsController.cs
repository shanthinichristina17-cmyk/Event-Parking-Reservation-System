using EventParkingSystem.API.Common;
using EventParkingSystem.API.DTOs;
using EventParkingSystem.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventParkingSystem.API.Controllers;

[ApiController]
[Route("api/admin/pro")]
[Authorize(Roles = Roles.Admin)]
public sealed class AdminProInsightsController : ControllerBase
{
    private readonly IProInsightsService _service;

    public AdminProInsightsController(IProInsightsService service) =>
        _service = service;

    [HttpGet("operations-alerts")]
    public async Task<ActionResult<List<AdminOperationsAlertResponse>>> OperationsAlerts() =>
        Ok(await _service.GetOperationsAlertsAsync());

    [HttpGet("capacity-utilization")]
    public async Task<ActionResult<List<EventCapacityUtilizationResponse>>> CapacityUtilization() =>
        Ok(await _service.GetCapacityUtilizationAsync());

    [HttpGet("payment-health")]
    public async Task<ActionResult<PaymentHealthResponse>> PaymentHealth(
        [FromQuery] int days = 30) =>
        Ok(await _service.GetPaymentHealthAsync(days));

    [HttpGet("revenue-opportunity")]
    public async Task<ActionResult<List<EventRevenueOpportunityItem>>> RevenueOpportunity() =>
        Ok(await _service.GetRevenueOpportunityAsync());

    [HttpGet("customer-segments")]
    public async Task<ActionResult<CustomerSegmentsResponse>> CustomerSegments() =>
        Ok(await _service.GetCustomerSegmentsAsync());

    [HttpGet("event-readiness")]
    public async Task<ActionResult<List<EventReadinessResponse>>> EventReadiness(
        [FromQuery] int days = 30) =>
        Ok(await _service.GetEventReadinessAsync(days));
}
