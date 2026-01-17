using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics;

namespace NppApi.Middleware;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "An unhandled exception occurred: {Message}", exception.Message);

        var problemDetails = new
        {
            status = GetStatusCode(exception),
            title = GetTitle(exception),
            detail = GetDetail(exception),
            instance = httpContext.Request.Path
        };

        httpContext.Response.ContentType = "application/json";
        httpContext.Response.StatusCode = problemDetails.status;

        await httpContext.Response.WriteAsync(
            JsonSerializer.Serialize(problemDetails),
            cancellationToken
        );

        return true;
    }

    private static int GetStatusCode(Exception exception) => exception switch
    {
        ArgumentException => (int)HttpStatusCode.BadRequest,
        UnauthorizedAccessException => (int)HttpStatusCode.Unauthorized,
        KeyNotFoundException => (int)HttpStatusCode.NotFound,
        InvalidOperationException => (int)HttpStatusCode.Conflict,
        _ => (int)HttpStatusCode.InternalServerError
    };

    private static string GetTitle(Exception exception) => exception switch
    {
        ArgumentException => "Bad Request",
        UnauthorizedAccessException => "Unauthorized",
        KeyNotFoundException => "Not Found",
        InvalidOperationException => "Conflict",
        _ => "Internal Server Error"
    };

    private static string GetDetail(Exception exception)
    {
        // U development vraćamo detalje, u production samo generičku poruku
        return exception is ArgumentException or KeyNotFoundException
            ? exception.Message
            : "An error occurred while processing your request.";
    }
}