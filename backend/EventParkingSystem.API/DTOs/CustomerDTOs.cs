using System.ComponentModel.DataAnnotations;

namespace EventParkingSystem.API.DTOs;

public sealed class RegisterCustomerRequest
{
    [Required, StringLength(150, MinimumLength = 2)] public string FullName { get; set; } = string.Empty;
    [Required, EmailAddress] public string Email { get; set; } = string.Empty;
    [Phone] public string? Phone { get; set; }
    [Required, MinLength(8), MaxLength(100)] public string Password { get; set; } = string.Empty;
}

public sealed class LoginRequest
{
    [Required, EmailAddress] public string Email { get; set; } = string.Empty;
    [Required] public string Password { get; set; } = string.Empty;
}

public sealed class ForgotPasswordRequest { [Required, EmailAddress] public string Email { get; set; } = string.Empty; }
public sealed class ResendVerificationRequest { [Required, EmailAddress] public string Email { get; set; } = string.Empty; }
public sealed class ResetPasswordRequest
{
    [Required] public string Token { get; set; } = string.Empty;
    [Required, MinLength(8), MaxLength(100)] public string NewPassword { get; set; } = string.Empty;
}

public sealed class UpdateCustomerProfileRequest
{
    [Required, StringLength(150, MinimumLength = 2)] public string FullName { get; set; } = string.Empty;
    [Phone] public string? Phone { get; set; }
}

public record CustomerProfileResponse(int CustomerId, string FullName, string Email, string? Phone,
    string Role, string Status, bool EmailVerified, DateTime CreatedAt);
public record LoginResponse(string Token, DateTime ExpiresAtUtc, CustomerProfileResponse Customer);
public record CustomerListItemResponse(int CustomerId, string FullName, string Email, string? Phone,
    string Status, bool EmailVerified, DateTime CreatedAt);
public record CustomerProfileWithBookingsSummaryResponse(CustomerProfileResponse Profile,
    int TotalBookings, int UpcomingBookings, int CancelledBookings);
