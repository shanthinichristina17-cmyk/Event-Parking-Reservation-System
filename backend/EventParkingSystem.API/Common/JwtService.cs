using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using EventParkingSystem.API.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace EventParkingSystem.API.Common;

public sealed class JwtSettings
{
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public string Secret { get; set; } = string.Empty;
    public int ExpiryMinutes { get; set; } = 120;
}

public interface IJwtService
{
    string GenerateToken(Customer customer);
}

public sealed class JwtService : IJwtService
{
    private readonly JwtSettings _settings;
    private readonly byte[] _keyBytes;

    public JwtService(IOptions<JwtSettings> options)
    {
        _settings = options.Value ?? throw new InvalidOperationException("Jwt settings are missing.");

        if (string.IsNullOrWhiteSpace(_settings.Issuer))
            throw new InvalidOperationException("Jwt:Issuer is required.");
        if (string.IsNullOrWhiteSpace(_settings.Audience))
            throw new InvalidOperationException("Jwt:Audience is required.");
        if (string.IsNullOrWhiteSpace(_settings.Secret))
            throw new InvalidOperationException("Jwt:Secret is required.");
        if (_settings.ExpiryMinutes <= 0)
            throw new InvalidOperationException("Jwt:ExpiryMinutes must be greater than zero.");

        _keyBytes = Encoding.UTF8.GetBytes(_settings.Secret);
        if (_keyBytes.Length < 32)
            throw new InvalidOperationException("Jwt:Secret must be at least 32 bytes for HS256.");
    }

    public string GenerateToken(Customer customer)
    {
        ArgumentNullException.ThrowIfNull(customer);

        var fullName = string.IsNullOrWhiteSpace(customer.FullName) ? customer.Email : customer.FullName;
        var email = customer.Email ?? string.Empty;
        var role = string.IsNullOrWhiteSpace(customer.Role) ? Roles.Customer : customer.Role;

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, customer.CustomerId.ToString()),
            new(ClaimTypes.NameIdentifier, customer.CustomerId.ToString()),
            new(ClaimTypes.Name, fullName ?? string.Empty),
            new(ClaimTypes.Email, email),
            new(ClaimTypes.Role, role),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N"))
        };

        var now = DateTime.UtcNow;
        var key = new SymmetricSecurityKey(_keyBytes);
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Issuer = _settings.Issuer,
            Audience = _settings.Audience,
            NotBefore = now,
            IssuedAt = now,
            Expires = now.AddMinutes(_settings.ExpiryMinutes),
            SigningCredentials = credentials
        };

        var handler = new JwtSecurityTokenHandler();
        var token = handler.CreateToken(descriptor);
        return handler.WriteToken(token);
    }
}
