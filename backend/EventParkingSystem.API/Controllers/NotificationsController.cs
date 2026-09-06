using EventParkingSystem.API.Common;
using EventParkingSystem.API.DTOs;
using EventParkingSystem.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventParkingSystem.API.Controllers;

[ApiController]
[Route("api/notifications")]
[Authorize]
public sealed class NotificationsController : ControllerBase
{
    private readonly INotificationService _notifications;
    private readonly IAdminService _admin;

    public NotificationsController(
        INotificationService notifications,
        IAdminService admin)
    {
        _notifications = notifications;
        _admin = admin;
    }

    [HttpGet]
    public async Task<ActionResult<List<NotificationResponse>>> Mine() =>
        Ok(await _notifications.GetForCustomerAsync(User.CustomerId()));

    [HttpGet("unread-count")]
    public async Task<IActionResult> UnreadCount() =>
        Ok(new { count = await _admin.GetUnreadNotificationCountAsync(User.CustomerId()) });

    [HttpPut("{notificationId:int}/read")]
    [HttpPatch("{notificationId:int}/read")]
    public async Task<IActionResult> MarkRead(int notificationId)
    {
        await _notifications.MarkAsReadAsync(
            notificationId,
            User.CustomerId(),
            User.IsInRole(Roles.Admin));

        return NoContent();
    }

    [HttpPost("broadcast")]
    [HttpPost("/api/admin/notifications/broadcast")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Broadcast([FromBody] BroadcastNotificationRequest request)
    {
        await _admin.BroadcastNotificationAsync(request);
        return Ok(new { message = "Notification sent to all active customers." });
    }
}
