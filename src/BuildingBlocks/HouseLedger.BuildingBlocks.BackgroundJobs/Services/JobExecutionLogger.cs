using HouseLedger.BuildingBlocks.BackgroundJobs.Abstractions;
using Serilog;

namespace HouseLedger.BuildingBlocks.BackgroundJobs.Services;

/// <summary>
/// Wrapper service for logging job execution.
/// </summary>
public class JobExecutionLogger
{
    private readonly ILogger _logger;

    public JobExecutionLogger()
    {
        _logger = Log.ForContext<JobExecutionLogger>();
    }

    /// <summary>
    /// Logs the start of a job execution.
    /// </summary>
    public void LogJobStart(IBackgroundJob job)
    {
        _logger.Information(
            "Starting background job: {JobName} ({Description})",
            job.JobName,
            job.Description);
    }

    /// <summary>
    /// Logs successful job completion.
    /// </summary>
    public void LogJobSuccess(IBackgroundJob job, JobResult result, TimeSpan duration)
    {
        _logger.Information(
            "Background job completed successfully: {JobName} - {Message} (Duration: {Duration}ms)",
            job.JobName,
            result.Message ?? "No message",
            duration.TotalMilliseconds);

        if (result.Data != null && result.Data.Any())
        {
            _logger.Debug(
                "Job {JobName} result data: {@Data}",
                job.JobName,
                result.Data);
        }
    }

    /// <summary>
    /// Logs job failure.
    /// </summary>
    public void LogJobFailure(IBackgroundJob job, JobResult result, TimeSpan duration)
    {
        if (result.Exception != null)
        {
            _logger.Error(
                result.Exception,
                "Background job failed: {JobName} - {Message} (Duration: {Duration}ms)",
                job.JobName,
                result.Message,
                duration.TotalMilliseconds);
        }
        else
        {
            _logger.Warning(
                "Background job failed: {JobName} - {Message} (Duration: {Duration}ms)",
                job.JobName,
                result.Message,
                duration.TotalMilliseconds);
        }
    }
}
