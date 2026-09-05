using EventParkingSystem.API.Common;
using EventParkingSystem.API.DTOs;
using EventParkingSystem.API.Models;
using EventParkingSystem.API.Repositories;
using Microsoft.Extensions.Options;

namespace EventParkingSystem.API.Services;

public interface IAuthService
{
    Task<CustomerProfileResponse> RegisterAsync(RegisterCustomerRequest request);
    Task<LoginResponse> LoginAsync(LoginRequest request);
    Task VerifyEmailAsync(string token);
    Task ResendVerificationAsync(ResendVerificationRequest request);
    Task ForgotPasswordAsync(ForgotPasswordRequest request);
    Task ResetPasswordAsync(ResetPasswordRequest request);
}

public sealed class AuthService : IAuthService
{
    private readonly ICustomerRepository _customers;
    private readonly IJwtService _jwt;
    private readonly IEmailService _email;
    private readonly JwtSettings _jwtSettings;
    private readonly AuthSettings _authSettings;

    public AuthService(
        ICustomerRepository customers,
        IJwtService jwt,
        IEmailService email,
        IOptions<JwtSettings> jwtOptions,
        IOptions<AuthSettings> authOptions)
    {
        _customers = customers;
        _jwt = jwt;
        _email = email;
        _jwtSettings = jwtOptions.Value;
        _authSettings = authOptions.Value;
    }

    public async Task<CustomerProfileResponse> RegisterAsync(RegisterCustomerRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        if (await _customers.EmailExistsAsync(email))
            throw ApiException.Conflict("An account with this email already exists.");

        string? rawToken = null;
        var emailVerified = !_authSettings.RequireEmailVerification;

        if (_authSettings.RequireEmailVerification)
            rawToken = SecureTokenGenerator.GenerateRawToken();

        var customer = new Customer
        {
            FullName = request.FullName.Trim(),
            Email = email,
            Phone = string.IsNullOrWhiteSpace(request.Phone) ? null : request.Phone.Trim(),
            PasswordHash = PasswordHasher.Hash(request.Password),
            Role = Roles.Customer,
            Status = CustomerStatuses.Active,
            EmailVerified = emailVerified,
            EmailVerificationToken = rawToken is null ? null : SecureTokenGenerator.Hash(rawToken),
            EmailVerificationTokenExpiresAt = rawToken is null ? null : DateTime.UtcNow.AddHours(24)
        };

        await _customers.AddAsync(customer);
        await _customers.SaveChangesAsync();

        if (rawToken is not null)
            await _email.SendEmailVerificationAsync(customer.Email, customer.FullName, rawToken);

        return MapProfile(customer);
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var customer = await _customers.GetByEmailAsync(email);

        if (customer is null || !PasswordHasher.Verify(request.Password, customer.PasswordHash))
            throw ApiException.Unauthorized("Invalid email or password.");

        if (!string.Equals(customer.Status, CustomerStatuses.Active, StringComparison.OrdinalIgnoreCase))
            throw ApiException.Forbidden("This account is deactivated.");

        if (_authSettings.RequireEmailVerification && !customer.EmailVerified)
            throw ApiException.Forbidden("Verify your email before logging in.");

        var token = _jwt.GenerateToken(customer);
        return new LoginResponse(
            token,
            DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes),
            MapProfile(customer));
    }

    public async Task VerifyEmailAsync(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            throw ApiException.BadRequest("Verification token is required.");

        var customer = await _customers.GetByVerificationTokenAsync(SecureTokenGenerator.Hash(token));
        if (customer is null || customer.EmailVerificationTokenExpiresAt is null || customer.EmailVerificationTokenExpiresAt <= DateTime.UtcNow)
            throw ApiException.BadRequest("Verification link is invalid or expired.");

        customer.EmailVerified = true;
        customer.EmailVerificationToken = null;
        customer.EmailVerificationTokenExpiresAt = null;
        customer.UpdatedAt = DateTime.UtcNow;
        await _customers.SaveChangesAsync();
    }

    public async Task ResendVerificationAsync(ResendVerificationRequest request)
    {
        if (!_authSettings.RequireEmailVerification)
            return;

        var customer = await _customers.GetByEmailAsync(request.Email.Trim().ToLowerInvariant());
        if (customer is null || customer.EmailVerified)
            return;

        var raw = SecureTokenGenerator.GenerateRawToken();
        customer.EmailVerificationToken = SecureTokenGenerator.Hash(raw);
        customer.EmailVerificationTokenExpiresAt = DateTime.UtcNow.AddHours(24);
        customer.UpdatedAt = DateTime.UtcNow;
        await _customers.SaveChangesAsync();
        await _email.SendEmailVerificationAsync(customer.Email, customer.FullName, raw);
    }

    public async Task ForgotPasswordAsync(ForgotPasswordRequest request)
    {
        var customer = await _customers.GetByEmailAsync(request.Email.Trim().ToLowerInvariant());
        if (customer is null)
            return;

        var raw = SecureTokenGenerator.GenerateRawToken();
        customer.PasswordResetToken = SecureTokenGenerator.Hash(raw);
        customer.PasswordResetTokenExpiresAt = DateTime.UtcNow.AddMinutes(45);
        customer.UpdatedAt = DateTime.UtcNow;
        await _customers.SaveChangesAsync();
        await _email.SendPasswordResetAsync(customer.Email, customer.FullName, raw);
    }

    public async Task ResetPasswordAsync(ResetPasswordRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Token))
            throw ApiException.BadRequest("Password reset token is required.");

        var customer = await _customers.GetByResetTokenAsync(SecureTokenGenerator.Hash(request.Token));
        if (customer is null || customer.PasswordResetTokenExpiresAt is null || customer.PasswordResetTokenExpiresAt <= DateTime.UtcNow)
            throw ApiException.BadRequest("Password reset link is invalid or expired.");

        customer.PasswordHash = PasswordHasher.Hash(request.NewPassword);
        customer.PasswordResetToken = null;
        customer.PasswordResetTokenExpiresAt = null;
        customer.UpdatedAt = DateTime.UtcNow;
        await _customers.SaveChangesAsync();
    }

    internal static CustomerProfileResponse MapProfile(Customer c) =>
        new(c.CustomerId, c.FullName, c.Email, c.Phone, c.Role, c.Status, c.EmailVerified, c.CreatedAt);
}
