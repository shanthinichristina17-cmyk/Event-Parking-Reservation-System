using EventParkingSystem.API.Common;
using EventParkingSystem.API.DTOs;
using EventParkingSystem.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventParkingSystem.API.Controllers;

[ApiController]
[Route("api/bookings/{bookingId:int}/payment")]
[Authorize]
public sealed class PaymentsController : ControllerBase
{
    private readonly IPaymentService _service;
    public PaymentsController(IPaymentService service) => _service = service;

    [HttpPost("simulate")]
    public async Task<ActionResult<PaymentResponse>> Simulate(
        int bookingId,
        [FromBody] SimulatePaymentRequest request) =>
        Ok(await _service.SimulateAsync(
            bookingId,
            User.CustomerId(),
            User.IsInRole(Roles.Admin),
            request));

    [HttpGet]
    public async Task<ActionResult<PaymentResponse>> Get(int bookingId) =>
        Ok(await _service.GetAsync(
            bookingId,
            User.CustomerId(),
            User.IsInRole(Roles.Admin)));
}
