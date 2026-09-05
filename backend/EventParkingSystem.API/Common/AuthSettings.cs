namespace EventParkingSystem.API.Common;

public sealed class AuthSettings
{
    // Development default: false so Swagger testing works without an SMTP account.
    // Set true when real email verification is configured.
    public bool RequireEmailVerification { get; set; } = false;
}
