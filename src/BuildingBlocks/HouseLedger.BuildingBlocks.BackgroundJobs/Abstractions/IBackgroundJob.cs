namespace HouseLedger.BuildingBlocks.BackgroundJobs.Abstractions;

/// <summary>
/// Base interface for all background jobs.
/// </summary>
public interface IBackgroundJob
{
    /// <summary>
    /// Gets the unique name of this job.
    /// </summary>
    string JobName { get; }

    /// <summary>
    /// Gets a description of what this job does.
    /// </summary>
    string Description { get; }

    /// <summary>
    /// Executes the background job.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token to stop the job execution.</param>
    /// <returns>The result of the job execution.</returns>
    Task<JobResult> ExecuteAsync(CancellationToken cancellationToken = default);
}
