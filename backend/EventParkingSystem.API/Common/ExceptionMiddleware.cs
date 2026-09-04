using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace EventParkingSystem.API.Common;

public sealed class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;
    private readonly IWebHostEnvironment _environment;

    public ExceptionMiddleware(
        RequestDelegate next,
        ILogger<ExceptionMiddleware> logger,
        IWebHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ApiException ex)
        {
            await WriteErrorAsync(context, ex.StatusCode, ex.Message, ex.Details);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogWarning(ex, "Database concurrency conflict. TraceId={TraceId}", context.TraceIdentifier);
            await WriteErrorAsync(context, StatusCodes.Status409Conflict,
                "The selected resource changed while you were submitting. Refresh and try again.");
        }
        catch (DbUpdateException ex)
        {
            _logger.LogWarning(ex, "Database update conflict. TraceId={TraceId}", context.TraceIdentifier);
            await WriteErrorAsync(context, StatusCodes.Status409Conflict,
                "The request conflicts with data that was updated by another user. Refresh and try again.",
                _environment.IsDevelopment() ? ex.GetBaseException().Message : null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled API exception. TraceId={TraceId}", context.TraceIdentifier);
            await WriteErrorAsync(context, StatusCodes.Status500InternalServerError,
                "An unexpected server error occurred.",
                _environment.IsDevelopment() ? ex.GetBaseException().Message : null);
        }
    }

    private static async Task WriteErrorAsync(HttpContext context, int statusCode, string message, object? details = null)
    {
        if (context.Response.HasStarted)
            return;

        context.Response.Clear();
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        var payload = new
        {
            status = statusCode,
            message,
            details,
            traceId = context.TraceIdentifier,
            timestamp = DateTime.UtcNow
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(payload));
    }
}
