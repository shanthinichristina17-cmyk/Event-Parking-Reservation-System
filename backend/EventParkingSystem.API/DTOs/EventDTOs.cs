using System.ComponentModel.DataAnnotations;

namespace EventParkingSystem.API.DTOs;

public class CreateEventRequest
{
    [Required, StringLength(200)] public string Name { get; set; } = string.Empty;
    [Range(1, int.MaxValue)] public int VenueId { get; set; }
    [Range(1, int.MaxValue)] public int CategoryId { get; set; }
    public DateOnly EventDate { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    [Range(0, 100000000)] public decimal TicketPrice { get; set; }
    [Range(0, 100000000)] public decimal ParkingFee { get; set; }
    [Range(1, 100000)] public int Capacity { get; set; }
}
public sealed class UpdateEventRequest : CreateEventRequest { }
public record EventSearchFilter(string? Name, DateOnly? Date, int? VenueId, int? CategoryId, int Page = 1, int PageSize = 20);
public record EventResponse(int EventId, string Name, int VenueId, string VenueName, int CategoryId,
    string CategoryName, DateOnly EventDate, TimeOnly StartTime, TimeOnly EndTime, decimal TicketPrice,
    decimal ParkingFee, int Capacity, int SeatsBooked, int SeatsHeld, int SeatsAvailable, DateTime CreatedAt);
