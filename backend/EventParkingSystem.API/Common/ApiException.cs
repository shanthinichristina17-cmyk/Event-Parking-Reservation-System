namespace EventParkingSystem.API.Common;

public sealed class ApiException : Exception
{
    public int StatusCode { get; }
    public object? Details { get; }

    public ApiException(int statusCode, string message, object? details = null) : base(message)
    {
        StatusCode = statusCode;
        Details = details;
    }

    public static ApiException BadRequest(string message, object? details = null) => new(400, message, details);
    public static ApiException Unauthorized(string message = "Unauthorized.") => new(401, message);
    public static ApiException Forbidden(string message = "Forbidden.") => new(403, message);
    public static ApiException NotFound(string message) => new(404, message);
    public static ApiException Conflict(string message, object? details = null) => new(409, message, details);
}
