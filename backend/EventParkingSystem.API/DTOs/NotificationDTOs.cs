namespace EventParkingSystem.API.DTOs;
public record NotificationResponse(int NotificationId, string Type, string Message, bool IsRead, DateTime CreatedAt);
