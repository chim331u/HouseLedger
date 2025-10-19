using Hangfire;
using HouseLedger.BuildingBlocks.BackgroundJobs.Abstractions;

namespace HouseLedger.BuildingBlocks.BackgroundJobs.Services;

/// <summary>
/// Hangfire implementation of the job scheduler.
/// </summary>
public class HangfireJobScheduler : IJobScheduler
{
    private readonly IBackgroundJobClient _backgroundJobClient;
    private readonly IRecurringJobManager _recurringJobManager;

    public HangfireJobScheduler(
        IBackgroundJobClient backgroundJobClient,
        IRecurringJobManager recurringJobManager)
    {
        _backgroundJobClient = backgroundJobClient;
        _recurringJobManager = recurringJobManager;
    }

    /// <inheritdoc />
    public string Enqueue<TJob>(params object[] args) where TJob : IBackgroundJob
    {
        return _backgroundJobClient.Enqueue<TJob>(job => job.ExecuteAsync(CancellationToken.None));
    }

    /// <inheritdoc />
    public string Schedule<TJob>(TimeSpan delay, params object[] args) where TJob : IBackgroundJob
    {
        return _backgroundJobClient.Schedule<TJob>(
            job => job.ExecuteAsync(CancellationToken.None),
            delay);
    }

    /// <inheritdoc />
    public void AddOrUpdate<TJob>(TJob job) where TJob : IRecurringJob
    {
        _recurringJobManager.AddOrUpdate(
            job.JobId,
            job.Queue,
            () => job.ExecuteAsync(CancellationToken.None),
            job.CronExpression,
            new RecurringJobOptions
            {
                TimeZone = job.TimeZone
            });
    }

    /// <inheritdoc />
    public void RemoveIfExists(string jobId)
    {
        _recurringJobManager.RemoveIfExists(jobId);
    }

    /// <inheritdoc />
    public void TriggerJob(string jobId)
    {
        _recurringJobManager.Trigger(jobId);
    }
}
