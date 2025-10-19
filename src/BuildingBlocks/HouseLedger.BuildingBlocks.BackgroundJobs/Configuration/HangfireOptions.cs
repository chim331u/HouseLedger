namespace HouseLedger.BuildingBlocks.BackgroundJobs.Configuration;

/// <summary>
/// Configuration options for Hangfire background job processing.
/// </summary>
public class HangfireOptions
{
    /// <summary>
    /// Gets or sets the SQLite database connection string for Hangfire storage.
    /// </summary>
    public string ConnectionString { get; set; } = "Data Source=hangfire.db";

    /// <summary>
    /// Gets or sets the number of worker threads to process jobs.
    /// Default is 2 for NAS deployment (memory constraint).
    /// </summary>
    public int WorkerCount { get; set; } = 2;

    /// <summary>
    /// Gets or sets the number of automatic retry attempts for failed jobs.
    /// </summary>
    public int RetryAttempts { get; set; } = 3;

    /// <summary>
    /// Gets or sets the job queues and their processing order.
    /// </summary>
    public string[] Queues { get; set; } = new[] { "critical", "default", "maintenance" };

    /// <summary>
    /// Gets or sets the number of days to retain completed job records.
    /// Default is 7 days for NAS deployment (storage constraint).
    /// </summary>
    public int JobRetentionDays { get; set; } = 7;

    /// <summary>
    /// Gets or sets whether the Hangfire dashboard is enabled.
    /// </summary>
    public bool DashboardEnabled { get; set; } = true;

    /// <summary>
    /// Gets or sets the path for the Hangfire dashboard.
    /// </summary>
    public string DashboardPath { get; set; } = "/hangfire";

    /// <summary>
    /// Gets or sets the username for dashboard authentication.
    /// </summary>
    public string? DashboardUsername { get; set; }

    /// <summary>
    /// Gets or sets the password for dashboard authentication.
    /// </summary>
    public string? DashboardPassword { get; set; }
}
