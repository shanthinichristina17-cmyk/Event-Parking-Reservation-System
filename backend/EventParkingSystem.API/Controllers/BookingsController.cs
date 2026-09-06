using EventParkingSystem.API.Common;
using EventParkingSystem.API.DTOs;
using EventParkingSystem.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventParkingSystem.API.Controllers;

[ApiController]
[Route("api/bookings")]
[Authorize]
public sealed class BookingsController : ControllerBase
{
    private readonly IBookingService _bookings;
    private readonly ITicketService _tickets;

    public BookingsController(IBookingService bookings, ITicketService tickets)
    {
        _bookings = bookings;
        _tickets = tickets;
    }

    [HttpPost("hold")]
    [Authorize(Roles = Roles.Customer)]
    public async Task<ActionResult<BookingSummaryResponse>> Hold(
        [FromBody] HoldSeatsRequest request) =>
        Ok(await _bookings.HoldSeatsAsync(User.CustomerId(), request));

    [HttpPut("{bookingId:int}/parking")]
    [Authorize(Roles = Roles.Customer)]
    public async Task<ActionResult<BookingSummaryResponse>> Parking(
        int bookingId,
        [FromBody] SelectParkingRequest request) =>
        Ok(await _bookings.SelectParkingAsync(
            bookingId,
            User.CustomerId(),
            request));

    [HttpPost("{bookingId:int}/promo")]
    [Authorize(Roles = Roles.Customer)]
    public async Task<ActionResult<BookingSummaryResponse>> Promo(
        int bookingId,
        [FromBody] ApplyPromoRequest request) =>
        Ok(await _bookings.ApplyPromoAsync(
            bookingId,
            User.CustomerId(),
            request));

    [HttpGet("me")]
    [Authorize(Roles = Roles.Customer)]
    public async Task<ActionResult<List<BookingSummaryResponse>>> Mine(
        [FromQuery] string? tab = null) =>
        Ok(await _bookings.GetMineAsync(User.CustomerId(), tab));

    [HttpGet("{bookingId:int}")]
    public async Task<ActionResult<BookingSummaryResponse>> Get(int bookingId) =>
        Ok(await _bookings.GetAsync(
            bookingId,
            User.CustomerId(),
            User.IsInRole(Roles.Admin)));

    [HttpGet("{bookingId:int}/summary")]
    public async Task<ActionResult<BookingSummaryResponse>> Summary(int bookingId) =>
        Ok(await _bookings.GetAsync(
            bookingId,
            User.CustomerId(),
            User.IsInRole(Roles.Admin)));

    [HttpPost("{bookingId:int}/cancel")]
    public async Task<ActionResult<CancellationResponse>> Cancel(int bookingId) =>
        Ok(await _bookings.CancelAsync(
            bookingId,
            User.CustomerId(),
            User.IsInRole(Roles.Admin)));

    [HttpGet("{bookingId:int}/ticket")]
    public async Task<ActionResult<TicketResponse>> Ticket(int bookingId) =>
        Ok(await _tickets.GetTicketAsync(
            bookingId,
            User.CustomerId(),
            User.IsInRole(Roles.Admin)));

    [HttpGet("{bookingId:int}/ticket/qr")]
    public async Task<IActionResult> QrTicket(int bookingId)
    {
        var bytes = await _tickets.GenerateQrPngAsync(
            bookingId,
            User.CustomerId(),
            User.IsInRole(Roles.Admin));

        return File(bytes, "image/png", $"ticket-{bookingId}-qr.png");
    }
}
