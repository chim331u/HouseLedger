namespace HouseLedger.BuildingBlocks.BackgroundJobs.Abstractions;

/// <summary>
/// Interface for recurring background jobs that run on a schedule.
/// </summary>
public interface IRecurringJob : IBackgroundJob
{
    /// <summary>
    /// Gets the unique identifier for this recurring job.
    /// </summary>
    string JobId { get; }

    /// <summary>
    /// Gets the CRON expression defining when this job should run.
    /// </summary>
    /// <remarks>
    /// Examples:
    /// - "0 2 * * *" = Daily at 2:00 AM
    /// - "0 */1 * * *" = Every hour
    /// - "0 0 * * 0" = Weekly on Sunday at midnight
    /// - "0 0 1 * *" = Monthly on the 1st at midnight
    /// </remarks>
    string CronExpression { get; }

    /// <summary>
    /// Gets the timezone for the CRON expression.
    /// </summary>
    TimeZoneInfo TimeZone { get; }

    /// <summary>
    /// Gets the queue name this job should run in.
    /// </summary>
    string Queue { get; }
}
