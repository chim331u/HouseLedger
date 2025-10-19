using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;

namespace HouseLedger.BuildingBlocks.Logging;

/// <summary>
/// Configures Serilog for HouseLedger applications.
///
/// Configuration (from appsettings.json):
/// - File + Console output
/// - Rolling by date (30 days retention)
/// - Text to console, JSON to file
/// - Standard enrichment (machine, environment, version)
/// - Different log levels for Development/Production
/// - Sensitive data masking in production
/// </summary>
public static class SerilogConfiguration
{
    /// <summary>
    /// Configures Serilog from appsettings.json configuration.
    /// Call this in Program.cs before building the application.
    /// </summary>
    /// <param name="builder">WebApplicationBuilder</param>
    /// <param name="configureLogger">Optional: additional logger configuration</param>
    public static void ConfigureSerilog(
        this WebApplicationBuilder builder,
        Action<LoggerConfiguration>? configureLogger = null)
    {
        var configuration = builder.Configuration;
        var environment = builder.Environment;

        // Create logger configuration
        var loggerConfig = new LoggerConfiguration()
            // Read from appsettings.json
            .ReadFrom.Configuration(configuration)

            // Enrich with standard properties (Question 8: Standard enrichment)
            .Enrich.FromLogContext()
            .Enrich.WithMachineName()
            .Enrich.WithEnvironmentName()
            .Enrich.WithProperty("Application", "HouseLedger")
            .Enrich.WithProperty("Version", GetApplicationVersion());

        // Console output (Question 1: File + Console, Question 7: Text to console)
        loggerConfig.WriteTo.Console(
            outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}",
            restrictedToMinimumLevel: GetMinimumLogLevel(configuration, environment, "Console"));

        // File output (Question 1: File + Console, Question 7: JSON to file)
        var logPath = GetLogPath(configuration);

        loggerConfig.WriteTo.File(
            new CompactJsonFormatter(), // JSON format for files
            path: Path.Combine(logPath, "houseledger-.json"),
            rollingInterval: RollingInterval.Day, // Question 3: Rolling by date
            retainedFileCountLimit: 30, // Question 3: Keep 30 days
            restrictedToMinimumLevel: GetMinimumLogLevel(configuration, environment, "File"));

        // Question 6: Sensitive data masking (development vs production)
        if (environment.IsProduction())
        {
            // Production: mask sensitive data
            loggerConfig.Filter.ByExcluding(evt =>
                ContainsSensitiveData(evt));
        }

        // Apply custom configuration if provided
        configureLogger?.Invoke(loggerConfig);

        // Create logger
        Log.Logger = loggerConfig.CreateLogger();

        // Use Serilog for ASP.NET Core logging
        builder.Host.UseSerilog();

        // Log application startup
        Log.Information("HouseLedger application starting up in {Environment} environment",
            environment.EnvironmentName);
    }

    /// <summary>
    /// Ensures Serilog is properly closed when application shuts down.
    /// Call this after app.Run() in Program.cs
    /// </summary>
    public static void CloseAndFlushSerilog()
    {
        Log.Information("HouseLedger application shutting down");
        Log.CloseAndFlush();
    }

    /// <summary>
    /// Gets the minimum log level from configuration.
    /// Question 4: Different levels for Development/Production
    /// </summary>
    private static LogEventLevel GetMinimumLogLevel(
        IConfiguration configuration,
        IHostEnvironment environment,
        string sinkName)
    {
        // Try to get from appsettings.json: Serilog:MinimumLevel:Override:{SinkName}
        var sinkLevelKey = $"Serilog:MinimumLevel:Override:{sinkName}";
        var sinkLevel = configuration[sinkLevelKey];

        if (!string.IsNullOrEmpty(sinkLevel) &&
            Enum.TryParse<LogEventLevel>(sinkLevel, out var parsedSinkLevel))
        {
            return parsedSinkLevel;
        }

        // Fallback to general minimum level
        var generalLevelKey = "Serilog:MinimumLevel:Default";
        var generalLevel = configuration[generalLevelKey];

        if (!string.IsNullOrEmpty(generalLevel) &&
            Enum.TryParse<LogEventLevel>(generalLevel, out var parsedGeneralLevel))
        {
            return parsedGeneralLevel;
        }

        // Final fallback based on environment
        return environment.IsDevelopment()
            ? LogEventLevel.Debug
            : LogEventLevel.Information;
    }

    /// <summary>
    /// Gets the log file path from configuration.
    /// Question 2: From appsettings.json, different for dev/prod
    /// </summary>
    private static string GetLogPath(IConfiguration configuration)
    {
        var configuredPath = configuration["Serilog:LogPath"];

        if (!string.IsNullOrEmpty(configuredPath))
        {
            // Use path from configuration
            var expandedPath = Environment.ExpandEnvironmentVariables(configuredPath);

            // Create directory if it doesn't exist
            if (!Directory.Exists(expandedPath))
            {
                Directory.CreateDirectory(expandedPath);
            }

            return expandedPath;
        }

        // Default fallback
        var defaultPath = Path.Combine(Directory.GetCurrentDirectory(), "logs");
        if (!Directory.Exists(defaultPath))
        {
            Directory.CreateDirectory(defaultPath);
        }

        return defaultPath;
    }

    /// <summary>
    /// Gets the application version for enrichment.
    /// Question 8: Standard enrichment includes version
    /// </summary>
    private static string GetApplicationVersion()
    {
        var version = typeof(SerilogConfiguration).Assembly
            .GetName()
            .Version?
            .ToString() ?? "1.0.0";

        return version;
    }

    /// <summary>
    /// Checks if log event contains sensitive data.
    /// Question 6: Log everything in development, redact in production
    /// </summary>
    private static bool ContainsSensitiveData(LogEvent logEvent)
    {
        // List of sensitive property names to exclude
        var sensitiveProperties = new[]
        {
            "password",
            "pwd",
            "secret",
            "token",
            "apikey",
            "api_key",
            "authorization",
            "auth",
            "creditcard",
            "cardnumber",
            "cvv",
            "ssn",
            "pin"
        };

        // Check if message or properties contain sensitive keywords
        var message = logEvent.RenderMessage().ToLowerInvariant();

        if (sensitiveProperties.Any(prop => message.Contains(prop)))
        {
            return true;
        }

        // Check properties
        foreach (var property in logEvent.Properties)
        {
            var propertyName = property.Key.ToLowerInvariant();
            if (sensitiveProperties.Any(prop => propertyName.Contains(prop)))
            {
                return true;
            }
        }

        return false;
    }
}
