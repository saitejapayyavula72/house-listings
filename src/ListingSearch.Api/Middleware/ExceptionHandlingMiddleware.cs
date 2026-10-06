using System.Text.Json;

namespace ListingSearch.Api.Middleware;

/// <summary>
/// Converts unexpected exceptions into a consistent JSON error response instead of exposing stack traces.
/// </summary>
public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            _logger.LogInformation("Request was cancelled by the client.");
        }
        catch (FileNotFoundException ex)
        {
            _logger.LogError(ex, "Listing data file could not be found.");
            await WriteErrorAsync(context, StatusCodes.Status500InternalServerError, "Listing data file is unavailable.");
        }
        catch (InvalidDataException ex)
        {
            _logger.LogError(ex, "Listing data file is invalid.");
            await WriteErrorAsync(context, StatusCodes.Status500InternalServerError, "Listing data file is invalid.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception while processing request.");
            await WriteErrorAsync(context, StatusCodes.Status500InternalServerError, "An unexpected error occurred while processing the request.");
        }
    }

    private static async Task WriteErrorAsync(HttpContext context, int statusCode, string message)
    {
        if (context.Response.HasStarted)
        {
            return;
        }

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        var body = new
        {
            error = message,
            traceId = context.TraceIdentifier
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(body));
    }
}
