namespace HouseLedger.BuildingBlocks.BackgroundJobs.Abstractions;

/// <summary>
/// Represents the result of a background job execution.
/// </summary>
public class JobResult
{
    public bool IsSuccess { get; init; }
    public string? Message { get; init; }
    public Exception? Exception { get; init; }
    public Dictionary<string, object>? Data { get; init; }

    /// <summary>
    /// Creates a successful job result.
    /// </summary>
    public static JobResult Success(string? message = null, Dictionary<string, object>? data = null)
    {
        return new JobResult
        {
            IsSuccess = true,
            Message = message,
            Data = data
        };
    }

    /// <summary>
    /// Creates a failed job result.
    /// </summary>
    public static JobResult Failure(string message, Exception? exception = null)
    {
        return new JobResult
        {
            IsSuccess = false,
            Message = message,
            Exception = exception
        };
    }

    /// <summary>
    /// Creates a failed job result from an exception.
    /// </summary>
    public static JobResult FromException(Exception exception)
    {
        return new JobResult
        {
            IsSuccess = false,
            Message = exception.Message,
            Exception = exception
        };
    }
}
