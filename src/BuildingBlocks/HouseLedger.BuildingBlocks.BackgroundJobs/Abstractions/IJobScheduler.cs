namespace HouseLedger.BuildingBlocks.BackgroundJobs.Abstractions;

/// <summary>
/// Service for scheduling and managing background jobs.
/// </summary>
public interface IJobScheduler
{
    /// <summary>
    /// Enqueues a job to run immediately in the background.
    /// </summary>
    /// <typeparam name="TJob">The type of job to enqueue.</typeparam>
    /// <param name="args">Arguments to pass to the job.</param>
    /// <returns>The unique identifier of the enqueued job.</returns>
    string Enqueue<TJob>(params object[] args) where TJob : IBackgroundJob;

    /// <summary>
    /// Schedules a job to run after a specified delay.
    /// </summary>
    /// <typeparam name="TJob">The type of job to schedule.</typeparam>
    /// <param name="delay">The delay before the job should run.</param>
    /// <param name="args">Arguments to pass to the job.</param>
    /// <returns>The unique identifier of the scheduled job.</returns>
    string Schedule<TJob>(TimeSpan delay, params object[] args) where TJob : IBackgroundJob;

    /// <summary>
    /// Adds or updates a recurring job.
    /// </summary>
    /// <typeparam name="TJob">The type of recurring job.</typeparam>
    /// <param name="job">The recurring job instance.</param>
    void AddOrUpdate<TJob>(TJob job) where TJob : IRecurringJob;

    /// <summary>
    /// Removes a recurring job if it exists.
    /// </summary>
    /// <param name="jobId">The unique identifier of the job to remove.</param>
    void RemoveIfExists(string jobId);

    /// <summary>
    /// Triggers a recurring job to run immediately.
    /// </summary>
    /// <param name="jobId">The unique identifier of the job to trigger.</param>
    void TriggerJob(string jobId);
}
