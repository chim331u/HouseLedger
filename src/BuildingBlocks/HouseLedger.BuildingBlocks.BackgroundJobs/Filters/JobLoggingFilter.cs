using Hangfire.Common;
using Hangfire.Logging;
using Hangfire.Server;
using Hangfire.States;
using Hangfire.Storage;
using Serilog;
using ILogger = Serilog.ILogger;

namespace HouseLedger.BuildingBlocks.BackgroundJobs.Filters;

/// <summary>
/// Filter that logs job execution lifecycle events.
/// </summary>
public class JobLoggingFilter : JobFilterAttribute, IServerFilter, IElectStateFilter, IApplyStateFilter
{
    private static readonly ILogger Logger = Log.ForContext<JobLoggingFilter>();

    public void OnPerforming(PerformingContext filterContext)
    {
        Logger.Information(
            "Starting job execution: {JobId} - {JobType}",
            filterContext.BackgroundJob.Id,
            filterContext.BackgroundJob.Job.Type.Name);
    }

    public void OnPerformed(PerformedContext filterContext)
    {
        if (filterContext.Exception == null)
        {
            Logger.Information(
                "Job executed successfully: {JobId} - {JobType}",
                filterContext.BackgroundJob.Id,
                filterContext.BackgroundJob.Job.Type.Name);
        }
        else
        {
            Logger.Error(
                filterContext.Exception,
                "Job execution failed: {JobId} - {JobType}",
                filterContext.BackgroundJob.Id,
                filterContext.BackgroundJob.Job.Type.Name);
        }
    }

    public void OnStateElection(ElectStateContext context)
    {
        if (context.CandidateState is FailedState failedState)
        {
            Logger.Warning(
                "Job {JobId} is transitioning to Failed state: {Reason}",
                context.BackgroundJob.Id,
                failedState.Reason);
        }
    }

    public void OnStateApplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
    {
        Logger.Debug(
            "Job {JobId} state changed to: {StateName}",
            context.BackgroundJob.Id,
            context.NewState.Name);
    }

    public void OnStateUnapplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
    {
        // Not used
    }
}
