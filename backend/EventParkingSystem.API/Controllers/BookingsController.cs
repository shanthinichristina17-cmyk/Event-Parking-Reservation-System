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

    [HttpPost]
    [Authorize(Roles = Roles.Customer)]
    public async Task<ActionResult<BookingResponse>> Create([FromBody] CreateBookingRequest request) =>
        Ok(await _bookings.CreateAsync(User.CustomerId(), request));

    [HttpGet("me")]
    [Authorize(Roles = Roles.Customer)]
    public async Task<ActionResult<List<BookingResponse>>> Mine() =>
        Ok(await _bookings.GetMineAsync(User.CustomerId()));

    [HttpGet("{bookingId:int}")]
    public async Task<ActionResult<BookingResponse>> Get(int bookingId) =>
        Ok(await _bookings.GetAsync(bookingId, User.CustomerId(), User.IsInRole(Roles.Admin)));

    [HttpPost("{bookingId:int}/cancel")]
    public async Task<ActionResult<BookingResponse>> Cancel(int bookingId) =>
        Ok(await _bookings.CancelAsync(bookingId, User.CustomerId(), User.IsInRole(Roles.Admin)));

    [HttpGet("{bookingId:int}/ticket/qr")]
    public async Task<IActionResult> QrTicket(int bookingId)
    {
        var png = await _tickets.GenerateQrPngAsync(
            bookingId,
            User.CustomerId(),
            User.IsInRole(Roles.Admin));

        return File(png, "image/png", $"booking-{bookingId}-qr.png");
    }
}
