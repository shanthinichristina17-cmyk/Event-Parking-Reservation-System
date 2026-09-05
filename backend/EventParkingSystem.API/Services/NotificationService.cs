using EventParkingSystem.API.Common;
using EventParkingSystem.API.DTOs;
using EventParkingSystem.API.Models;
using EventParkingSystem.API.Repositories;

namespace EventParkingSystem.API.Services;

public interface INotificationService
{
    Task CreateAsync(int customerId, string type, string message);
    Task CreateIfMissingAsync(int customerId, string type, string message);
    Task<List<NotificationResponse>> GetForCustomerAsync(int customerId);
    Task MarkAsReadAsync(int notificationId, int requesterId, bool isAdmin);
}

public sealed class NotificationService : INotificationService
{
    private readonly INotificationRepository _notifications;
    public NotificationService(INotificationRepository notifications) => _notifications = notifications;

    public async Task CreateAsync(int customerId, string type, string message)
    {
        await _notifications.AddAsync(new Notification { CustomerId = customerId, Type = type, Message = message });
        await _notifications.SaveChangesAsync();
    }

    public async Task CreateIfMissingAsync(int customerId, string type, string message)
    {
        if (await _notifications.ExistsAsync(customerId, type, message)) return;
        await CreateAsync(customerId, type, message);
    }

    public async Task<List<NotificationResponse>> GetForCustomerAsync(int customerId) =>
        (await _notifications.GetForCustomerAsync(customerId))
        .Select(x => new NotificationResponse(x.NotificationId, x.Type, x.Message, x.IsRead, x.CreatedAt)).ToList();

    public async Task MarkAsReadAsync(int notificationId, int requesterId, bool isAdmin)
    {
        var notification = await _notifications.GetByIdAsync(notificationId) ?? throw ApiException.NotFound("Notification not found.");
        if (!isAdmin && notification.CustomerId != requesterId)
            throw ApiException.Forbidden("You can only update your own notifications.");
        notification.IsRead = true;
        await _notifications.SaveChangesAsync();
    }
}
