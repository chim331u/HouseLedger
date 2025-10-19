using System.Diagnostics;

namespace HouseLedger.Services.Finance.Api.Middleware;

/// <summary>
/// Middleware that logs all HTTP requests and responses with timing information.
/// Uses Serilog for structured logging.
/// </summary>
public class RequestResponseLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestResponseLoggingMiddleware> _logger;

    public RequestResponseLoggingMiddleware(
        RequestDelegate next,
        ILogger<RequestResponseLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var requestPath = context.Request.Path;
        var requestMethod = context.Request.Method;

        _logger.LogInformation(
            "HTTP {Method} {Path} started",
            requestMethod,
            requestPath);

        try
        {
            await _next(context);

            stopwatch.Stop();

            var statusCode = context.Response.StatusCode;
            var logLevel = statusCode >= 400 ? LogLevel.Warning : LogLevel.Information;

            _logger.Log(
                logLevel,
                "HTTP {Method} {Path} responded {StatusCode} in {ElapsedMilliseconds}ms",
                requestMethod,
                requestPath,
                statusCode,
                stopwatch.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            _logger.LogError(
                ex,
                "HTTP {Method} {Path} failed after {ElapsedMilliseconds}ms",
                requestMethod,
                requestPath,
                stopwatch.ElapsedMilliseconds);

            throw;
        }
    }
}
