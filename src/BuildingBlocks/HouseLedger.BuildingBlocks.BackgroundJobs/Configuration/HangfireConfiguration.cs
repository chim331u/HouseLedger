using Hangfire;
using Hangfire.Storage.SQLite;
using HouseLedger.BuildingBlocks.BackgroundJobs.Abstractions;
using HouseLedger.BuildingBlocks.BackgroundJobs.Filters;
using HouseLedger.BuildingBlocks.BackgroundJobs.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace HouseLedger.BuildingBlocks.BackgroundJobs.Configuration;

/// <summary>
/// Extension methods for configuring Hangfire background jobs.
/// </summary>
public static class HangfireConfiguration
{
    /// <summary>
    /// Adds Hangfire background job services to the service collection.
    /// </summary>
    public static IServiceCollection AddHouseLedgerBackgroundJobs(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<HangfireOptions>? configureOptions = null)
    {
        // Bind configuration
        var options = new HangfireOptions();
        configuration.GetSection("Hangfire").Bind(options);
        configureOptions?.Invoke(options);

        // Resolve connection string: if it's a name, look it up in ConnectionStrings section
        var connectionString = options.ConnectionString;
        var actualConnectionString = configuration.GetConnectionString(connectionString) ?? connectionString;

        // Extract database path from connection string
        // Hangfire.Storage.SQLite uses just the file path, not "Data Source=..." format
        string dbPath;

        if (actualConnectionString.Contains("Data Source", StringComparison.OrdinalIgnoreCase))
        {
            dbPath = ExtractDatabasePath(actualConnectionString) ?? actualConnectionString;
        }
        else
        {
            // If it's just a path, use it directly
            dbPath = actualConnectionString;
        }

        // Convert relative path to absolute
        if (!Path.IsPathRooted(dbPath))
        {
            dbPath = Path.GetFullPath(dbPath);
        }

        // Ensure the directory exists
        var directory = Path.GetDirectoryName(dbPath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        // Register options
        services.Configure<HangfireOptions>(config =>
        {
            config.ConnectionString = options.ConnectionString;
            config.WorkerCount = options.WorkerCount;
            config.RetryAttempts = options.RetryAttempts;
            config.Queues = options.Queues;
            config.JobRetentionDays = options.JobRetentionDays;
            config.DashboardEnabled = options.DashboardEnabled;
            config.DashboardPath = options.DashboardPath;
            config.DashboardUsername = options.DashboardUsername;
            config.DashboardPassword = options.DashboardPassword;
        });

        // Add Hangfire services
        services.AddHangfire(config => config
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UseSQLiteStorage(dbPath, new SQLiteStorageOptions
            {
                QueuePollInterval = TimeSpan.FromSeconds(15)
            })
            .UseFilter(new AutomaticRetryFilter { Attempts = options.RetryAttempts })
            .UseFilter(new JobLoggingFilter()));

        // Add Hangfire server
        services.AddHangfireServer(serverOptions =>
        {
            serverOptions.WorkerCount = options.WorkerCount;
            serverOptions.Queues = options.Queues;
            serverOptions.ServerTimeout = TimeSpan.FromMinutes(5);
            serverOptions.ShutdownTimeout = TimeSpan.FromMinutes(1);
        });

        // Register job scheduler
        services.AddSingleton<IJobScheduler, HangfireJobScheduler>();

        return services;
    }

    /// <summary>
    /// Extracts the database file path from a SQLite connection string.
    /// </summary>
    private static string? ExtractDatabasePath(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            return null;

        // Handle "Data Source=path" format
        var match = System.Text.RegularExpressions.Regex.Match(
            connectionString,
            @"Data Source\s*=\s*([^;]+)",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);

        if (match.Success)
        {
            var path = match.Groups[1].Value.Trim();
            // Convert relative path to absolute if needed
            if (!Path.IsPathRooted(path))
            {
                path = Path.GetFullPath(path);
            }
            return path;
        }

        return null;
    }

    /// <summary>
    /// Configures Hangfire dashboard in the application pipeline.
    /// </summary>
    public static IApplicationBuilder UseHouseLedgerBackgroundJobs(
        this IApplicationBuilder app,
        Action<Hangfire.DashboardOptions>? configureDashboard = null)
    {
        var options = app.ApplicationServices.GetRequiredService<IOptions<HangfireOptions>>().Value;

        if (!options.DashboardEnabled)
        {
            return app;
        }

        var dashboardOptions = new Hangfire.DashboardOptions
        {
            DashboardTitle = "HouseLedger Background Jobs",
            Authorization = new[] { new HangfireDashboardAuthFilter(
                app.ApplicationServices.GetRequiredService<IOptions<HangfireOptions>>()) }
        };

        configureDashboard?.Invoke(dashboardOptions);

        app.UseHangfireDashboard(options.DashboardPath, dashboardOptions);

        return app;
    }

    /// <summary>
    /// Registers all recurring jobs found in the service collection.
    /// </summary>
    public static IApplicationBuilder RegisterRecurringJobs(
        this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var jobScheduler = scope.ServiceProvider.GetRequiredService<IJobScheduler>();
        var recurringJobs = scope.ServiceProvider.GetServices<IRecurringJob>();

        foreach (var job in recurringJobs)
        {
            jobScheduler.AddOrUpdate(job);
        }

        return app;
    }
}
