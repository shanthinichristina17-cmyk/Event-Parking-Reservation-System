using System.Security.Claims;

namespace EventParkingSystem.API.Common;

public static class ClaimsPrincipalExtensions
{
    public static int CustomerId(this ClaimsPrincipal user)
    {
        var value = user.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(value, out var id) ? id : throw ApiException.Unauthorized("Invalid authentication token.");
    }
}
