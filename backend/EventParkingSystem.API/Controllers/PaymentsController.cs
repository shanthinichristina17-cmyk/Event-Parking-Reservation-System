using EventParkingSystem.API.Common;
using EventParkingSystem.API.DTOs;
using EventParkingSystem.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventParkingSystem.API.Controllers;

[ApiController]
[Route("api/payments")]
[Authorize]
public sealed class PaymentsController : ControllerBase
{
    private readonly IPaymentService _service;
    public PaymentsController(IPaymentService service) => _service = service;

    [HttpPost]
    public async Task<ActionResult<PaymentResponse>> Pay([FromBody] CreatePaymentRequest request) =>
        Ok(await _service.PayAsync(
            User.CustomerId(),
            User.IsInRole(Roles.Admin),
            request));

    [HttpGet("booking/{bookingId:int}")]
    public async Task<ActionResult<PaymentResponse>> GetForBooking(int bookingId) =>
        Ok(await _service.GetForBookingAsync(
            bookingId,
            User.CustomerId(),
            User.IsInRole(Roles.Admin)));
}
