using System.ComponentModel.DataAnnotations;

namespace EventParkingSystem.API.DTOs;

public class CreateVenueRequest
{
    [Required, StringLength(150)] public string Name { get; set; } = string.Empty;
    [Required, StringLength(300)] public string Address { get; set; } = string.Empty;
    [Range(1, 100000)] public int Capacity { get; set; }
}
public sealed class UpdateVenueRequest : CreateVenueRequest { }
public record VenueResponse(int VenueId, string Name, string Address, int Capacity, DateTime CreatedAt);
public record VenueAvailabilityResponse(int VenueId, string Name, bool IsAvailable, string? ConflictingEventName);

public class CreateCategoryRequest { [Required, StringLength(100)] public string Name { get; set; } = string.Empty; }
public sealed class UpdateCategoryRequest : CreateCategoryRequest { }
public record CategoryResponse(int CategoryId, string Name, DateTime CreatedAt);
