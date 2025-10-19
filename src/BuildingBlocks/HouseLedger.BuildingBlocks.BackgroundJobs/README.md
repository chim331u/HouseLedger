# HouseLedger.BuildingBlocks.BackgroundJobs

Background job processing infrastructure for HouseLedger using Hangfire.

## Overview

This BuildingBlock provides a clean abstraction layer over Hangfire for scheduling and executing background jobs in HouseLedger. It includes:

- **Job Abstractions**: Interfaces for fire-and-forget and recurring jobs
- **Job Scheduler**: Service for enqueueing and scheduling jobs
- **Automatic Retry**: Configurable retry logic for failed jobs
- **Job Logging**: Comprehensive logging of job execution
- **Dashboard**: Web-based monitoring interface (optional)
- **Memory Optimized**: Configured for NAS deployment (1GB RAM constraint)

## Installation

### 1. Add Package Reference

```xml
<ItemGroup>
  <ProjectReference Include="..\..\BuildingBlocks\HouseLedger.BackgroundJobs\HouseLedger.BuildingBlocks.BackgroundJobs.csproj" />
</ItemGroup>
```

### 2. Configure in Program.cs

```csharp
using HouseLedger.BuildingBlocks.BackgroundJobs.Configuration;

// Add Hangfire services
builder.Services.AddHouseLedgerBackgroundJobs(builder.Configuration);

// ... build app ...

// Enable Hangfire dashboard
app.UseHouseLedgerBackgroundJobs();

// Register recurring jobs
app.RegisterRecurringJobs();
```

### 3. Add Configuration (appsettings.json)

```json
{
  "Hangfire": {
    "ConnectionString": "Data Source=../../../../data/housledger-jobs.db",
    "WorkerCount": 2,
    "RetryAttempts": 3,
    "Queues": ["critical", "default", "maintenance"],
    "JobRetentionDays": 7,
    "DashboardEnabled": true,
    "DashboardPath": "/hangfire",
    "DashboardUsername": "admin",
    "DashboardPassword": "your-secure-password"
  }
}
```

## Creating Jobs

### Fire-and-Forget Job

```csharp
public class SendEmailJob : IBackgroundJob
{
    public string JobName => "Send Email";
    public string Description => "Sends an email notification";

    public async Task<JobResult> ExecuteAsync(CancellationToken cancellationToken)
    {
        try
        {
            // Job logic here
            await SendEmailAsync(cancellationToken);

            return JobResult.Success("Email sent successfully");
        }
        catch (Exception ex)
        {
            return JobResult.FromException(ex);
        }
    }
}
```

### Recurring Job

```csharp
public class DailyReportJob : IRecurringJob
{
    public string JobName => "Daily Report";
    public string Description => "Generates daily financial report";
    public string JobId => "daily-report";
    public string CronExpression => "0 2 * * *"; // Daily at 2 AM
    public TimeZoneInfo TimeZone => TimeZoneInfo.Local;
    public string Queue => "default";

    public async Task<JobResult> ExecuteAsync(CancellationToken cancellationToken)
    {
        try
        {
            // Job logic here
            var report = await GenerateReportAsync(cancellationToken);

            return JobResult.Success($"Report generated with {report.ItemCount} items");
        }
        catch (Exception ex)
        {
            return JobResult.FromException(ex);
        }
    }
}
```

## Scheduling Jobs

### Enqueue (Run Immediately)

```csharp
public class MyController : ControllerBase
{
    private readonly IJobScheduler _jobScheduler;

    public MyController(IJobScheduler jobScheduler)
    {
        _jobScheduler = jobScheduler;
    }

    [HttpPost("send-email")]
    public IActionResult SendEmail()
    {
        var jobId = _jobScheduler.Enqueue<SendEmailJob>();
        return Ok(new { jobId });
    }
}
```

### Schedule (Delayed Execution)

```csharp
// Schedule job to run in 1 hour
var jobId = _jobScheduler.Schedule<SendEmailJob>(TimeSpan.FromHours(1));
```

### Register Recurring Job

```csharp
// In Startup/Program.cs
builder.Services.AddScoped<IRecurringJob, DailyReportJob>();

// Jobs are automatically registered when app.RegisterRecurringJobs() is called
```

## CRON Expressions

Common CRON patterns for recurring jobs:

| Expression | Description |
|------------|-------------|
| `"0 2 * * *"` | Daily at 2:00 AM |
| `"0 */1 * * *"` | Every hour |
| `"0 0 * * 0"` | Weekly on Sunday at midnight |
| `"0 0 1 * *"` | Monthly on the 1st at midnight |
| `"0 0 L * *"` | Monthly on the last day at midnight |
| `"*/15 * * * *"` | Every 15 minutes |

Use [crontab.guru](https://crontab.guru/) for testing CRON expressions.

## Job Queues

Jobs are processed in priority order:

1. **critical** - High priority (backups, health checks)
2. **default** - Normal priority (currency updates, reports)
3. **maintenance** - Low priority (cleanup, vacuum)

Specify queue in your recurring job:

```csharp
public string Queue => "critical";
```

## Dashboard

Access the Hangfire dashboard at: `http://localhost:5000/hangfire`

Default credentials:
- **Username**: From `Hangfire:DashboardUsername` config
- **Password**: From `Hangfire:DashboardPassword` config

**Note**: Disable dashboard in production if memory is constrained by setting `DashboardEnabled: false`.

## Memory Optimization (NAS Deployment)

For QNAP TS-231P (1GB RAM) deployment:

```json
{
  "Hangfire": {
    "WorkerCount": 2,           // Limit concurrent jobs
    "JobRetentionDays": 7,      // Keep only 7 days history
    "DashboardEnabled": false,  // Disable if memory tight
    "Queues": ["critical", "default", "maintenance"]
  }
}
```

Expected memory usage:
- **Hangfire Server**: 50-100 MB
- **2 Workers**: Minimal overhead
- **SQLite Storage**: ~10-20 MB

## Job Result Patterns

### Success with Data

```csharp
return JobResult.Success("Processed 150 items", new Dictionary<string, object>
{
    { "ItemCount", 150 },
    { "Duration", elapsed.TotalSeconds }
});
```

### Failure with Exception

```csharp
catch (HttpRequestException ex)
{
    return JobResult.Failure("Failed to fetch currency rates", ex);
}
```

### Exception-based Failure

```csharp
catch (Exception ex)
{
    return JobResult.FromException(ex);
}
```

## Dependency Injection

Jobs support constructor injection:

```csharp
public class UpdateCurrencyRatesJob : IRecurringJob
{
    private readonly ICurrencyService _currencyService;
    private readonly ILogger<UpdateCurrencyRatesJob> _logger;

    public UpdateCurrencyRatesJob(
        ICurrencyService currencyService,
        ILogger<UpdateCurrencyRatesJob> logger)
    {
        _currencyService = currencyService;
        _logger = logger;
    }

    // ... implement interface
}
```

Register job in DI container:

```csharp
builder.Services.AddScoped<IRecurringJob, UpdateCurrencyRatesJob>();
```

## Logging

All job executions are automatically logged:

- Job start/completion
- Execution duration
- Success/failure status
- Retry attempts
- Exceptions with stack traces

Logs are written using Serilog to configured sinks.

## Testing

### Unit Testing Jobs

```csharp
[Fact]
public async Task ExecuteAsync_ShouldReturnSuccess_WhenEmailSent()
{
    // Arrange
    var job = new SendEmailJob(mockEmailService.Object);

    // Act
    var result = await job.ExecuteAsync(CancellationToken.None);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.Equal("Email sent successfully", result.Message);
}
```

### Testing Scheduled Jobs

```csharp
[Fact]
public void RecurringJob_ShouldHaveCorrectSchedule()
{
    // Arrange
    var job = new DailyReportJob();

    // Assert
    Assert.Equal("0 2 * * *", job.CronExpression);
    Assert.Equal("daily-report", job.JobId);
    Assert.Equal("default", job.Queue);
}
```

## Troubleshooting

### Jobs Not Running

1. Check Hangfire dashboard for job status
2. Verify SQLite database connection
3. Check server logs for exceptions
4. Ensure worker count > 0

### High Memory Usage

1. Reduce `WorkerCount` to 1
2. Decrease `JobRetentionDays`
3. Disable dashboard if not needed
4. Check for memory leaks in job logic

### Jobs Failing

1. Check job logs for exceptions
2. Verify retry attempts configuration
3. Check external service availability
4. Test job logic in isolation

## Architecture

```
┌─────────────────────────────────────────┐
│          API Gateway / Host             │
├─────────────────────────────────────────┤
│  HangfireConfiguration.cs               │
│  - AddHouseLedgerBackgroundJobs()       │
│  - UseHouseLedgerBackgroundJobs()       │
│  - RegisterRecurringJobs()              │
└─────────────┬───────────────────────────┘
              │
              ▼
┌─────────────────────────────────────────┐
│     Background Jobs BuildingBlock       │
├─────────────────────────────────────────┤
│  Abstractions/                          │
│  - IBackgroundJob                       │
│  - IRecurringJob                        │
│  - IJobScheduler                        │
│  - JobResult                            │
│                                         │
│  Services/                              │
│  - HangfireJobScheduler                 │
│  - JobExecutionLogger                   │
│                                         │
│  Filters/                               │
│  - AutomaticRetryFilter                 │
│  - JobLoggingFilter                     │
└─────────────┬───────────────────────────┘
              │
              ▼
┌─────────────────────────────────────────┐
│            Hangfire Core                │
├─────────────────────────────────────────┤
│  - Job Queue Management                 │
│  - Worker Threads                       │
│  - SQLite Storage                       │
│  - Dashboard (Optional)                 │
└─────────────────────────────────────────┘
```

## References

- [Hangfire Documentation](https://docs.hangfire.io/)
- [Hangfire Best Practices](https://docs.hangfire.io/en/latest/best-practices.html)
- [Hangfire SQLite Storage](https://github.com/wanlitao/HangfireExtension)
- [CRON Expression Generator](https://crontab.guru/)

---

**Version**: 1.0.0
**Last Updated**: October 19, 2025
**Maintainer**: HouseLedger Team
