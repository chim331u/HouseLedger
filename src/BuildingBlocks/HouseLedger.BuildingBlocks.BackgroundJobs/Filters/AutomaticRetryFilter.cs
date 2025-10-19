using Hangfire.Common;
using Hangfire.States;
using Hangfire.Storage;

namespace HouseLedger.BuildingBlocks.BackgroundJobs.Filters;

/// <summary>
/// Filter that automatically retries failed jobs with configurable attempts.
/// </summary>
public class AutomaticRetryFilter : JobFilterAttribute, IElectStateFilter
{
    /// <summary>
    /// Gets or sets the maximum number of automatic retry attempts.
    /// </summary>
    public int Attempts { get; set; } = 3;

    /// <summary>
    /// Gets or sets the delay between retry attempts in seconds.
    /// </summary>
    public int DelayInSeconds { get; set; } = 60;

    public void OnStateElection(ElectStateContext context)
    {
        var failedState = context.CandidateState as FailedState;
        if (failedState == null)
        {
            return;
        }

        var retryAttempt = context.GetJobParameter<int>("RetryCount") + 1;

        if (retryAttempt <= Attempts)
        {
            context.SetJobParameter("RetryCount", retryAttempt);

            var delay = TimeSpan.FromSeconds(DelayInSeconds * retryAttempt);

            context.CandidateState = new ScheduledState(delay)
            {
                Reason = $"Retry attempt {retryAttempt} of {Attempts}: {failedState.Exception.Message}"
            };
        }
    }
}
