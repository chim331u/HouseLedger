# HouseLedger.BuildingBlocks.Logging

Serilog logging configuration for HouseLedger applications.

## Features

✅ **File + Console output**
- Console: Human-readable text (great for development)
- File: Structured JSON (great for parsing/analysis)

✅ **Configured via appsettings.json**
- Different log paths for Development/Production
- Different log levels for Development/Production
- Easy to change without recompiling

✅ **Rolling file management**
- One file per day: `housledger-20251018.json`
- Automatically keeps last 30 days
- Old files automatically deleted

✅ **Environment-specific behavior**
- **Development:** Debug level, logs everything (including sensitive data for debugging)
- **Production:** Information level, masks sensitive data (passwords, tokens, etc.)

✅ **Standard enrichment**
- Machine name
- Environment name (Development/Production)
- Application version
- Timestamp

✅ **Sensitive data protection**
- Production: Automatically excludes logs containing passwords, tokens, API keys, etc.
- Development: Logs everything for easier debugging

## Usage

### 1. Add reference to your API project

```bash
dotnet add reference ../../BuildingBlocks/HouseLedger.BuildingBlocks.Logging/HouseLedger.BuildingBlocks.Logging.csproj
```

### 2. Configure in Program.cs

```csharp
using HouseLedger.BuildingBlocks.Logging;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog (before building the app!)
builder.ConfigureSerilog();

// ... rest of your configuration ...

var app = builder.Build();

// ... your middleware ...

app.Run();

// Close Serilog on shutdown
SerilogConfiguration.CloseAndFlushSerilog();
```

### 3. Add Serilog section to appsettings.json

**appsettings.Development.json:**
```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Debug",
      "Override": {
        "Microsoft": "Information",
        "Microsoft.AspNetCore": "Warning",
        "Microsoft.EntityFrameworkCore": "Information"
      }
    },
    "LogPath": "logs"
  }
}
```

**appsettings.Production.json:**
```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "Microsoft.AspNetCore": "Warning",
        "Microsoft.EntityFrameworkCore": "Warning"
      }
    },
    "LogPath": "/var/log/housledger"
  }
}
```

### 4. Use ILogger in your code

```csharp
public class TransactionHandler
{
    private readonly ILogger<TransactionHandler> _logger;

    public TransactionHandler(ILogger<TransactionHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(CreateTransactionCommand command)
    {
        _logger.LogInformation("Creating transaction for account {AccountId} with amount {Amount}",
            command.AccountId, command.Amount);

        try
        {
            // ... your logic ...

            _logger.LogInformation("Transaction created successfully with ID {TransactionId}",
                transaction.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create transaction for account {AccountId}",
                command.AccountId);
            throw;
        }
    }
}
```

## Log Levels

| Level | When to use | Example |
|-------|-------------|---------|
| **Verbose** | Extremely detailed, trace every step | "Entering method X", "Variable Y = 123" |
| **Debug** | Detailed information for debugging | "Query returned 5 results", "Cache miss for key X" |
| **Information** | General application flow | "Transaction created", "User logged in" |
| **Warning** | Unexpected but handled situations | "Retry attempt 2/3", "Using fallback value" |
| **Error** | Errors and exceptions | "Failed to connect to database", "Invalid input" |
| **Fatal** | Critical errors causing shutdown | "Out of memory", "Cannot start application" |

## Configuration Options

### Log Path

Specify where logs should be written:

```json
{
  "Serilog": {
    "LogPath": "logs"  // Relative to application directory
  }
}
```

Or use absolute path:

```json
{
  "Serilog": {
    "LogPath": "/var/log/housledger"  // Absolute path (Linux/Mac)
  }
}
```

Or use environment variables:

```json
{
  "Serilog": {
    "LogPath": "%TEMP%\\HouseLedger\\Logs"  // Windows
  }
}
```

### Minimum Log Level

Control what gets logged:

```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",  // Default for all
      "Override": {
        "Microsoft": "Warning",  // Less noise from Microsoft libraries
        "MyNamespace": "Debug"   // More detail from your code
      }
    }
  }
}
```

## Output Formats

### Console Output (Human-Readable)

```
[14:32:15 INF] Transaction created successfully with ID 123 {"AccountId": 5, "Amount": 50.0}
[14:32:16 WRN] Retry attempt 2/3 for operation ImportTransactions
[14:32:17 ERR] Failed to send notification: Connection timeout
```

### File Output (JSON - Machine-Readable)

```json
{
  "@t": "2025-10-18T14:32:15.1234567Z",
  "@mt": "Transaction created successfully with ID {TransactionId}",
  "@l": "Information",
  "TransactionId": 123,
  "AccountId": 5,
  "Amount": 50.0,
  "MachineName": "SERVER01",
  "EnvironmentName": "Production",
  "Application": "HouseLedger",
  "Version": "1.0.0"
}
```

## Sensitive Data Protection

The following property names are automatically excluded in **Production**:
- password, pwd
- secret
- token
- apikey, api_key
- authorization, auth
- creditcard, cardnumber
- cvv
- ssn
- pin

**Development:** All data is logged (for debugging)
**Production:** Logs containing these keywords are excluded

## File Management

**Daily rolling:**
- `housledger-20251018.json`
- `housledger-20251019.json`
- etc.

**Automatic cleanup:**
- Files older than 30 days are automatically deleted
- No manual cleanup needed

**Location:**
- Development: `./logs/` (project directory)
- Production: `/var/log/housledger/` (or configured path)

## Advanced: Custom Configuration

If you need custom Serilog configuration:

```csharp
builder.ConfigureSerilog(config =>
{
    // Add additional sinks
    config.WriteTo.Seq("http://localhost:5341");

    // Add custom enrichers
    config.Enrich.WithProperty("CustomProperty", "Value");

    // Add custom filters
    config.Filter.ByExcluding(evt => evt.Level == LogEventLevel.Verbose);
});
```

## Troubleshooting

### Logs not appearing

1. Check `Serilog:MinimumLevel:Default` in appsettings.json
2. Verify log path exists and is writable
3. Check you called `builder.ConfigureSerilog()` BEFORE `builder.Build()`

### Permission denied on log path

Production path `/var/log/housledger/` requires permissions:

```bash
sudo mkdir -p /var/log/housledger
sudo chown youruser:yourgroup /var/log/housledger
```

Or use application directory:

```json
{
  "Serilog": {
    "LogPath": "logs"  // Relative to app directory
  }
}
```

### Too many log files

Adjust retention:

Change in `SerilogConfiguration.cs`:
```csharp
retainedFileCountLimit: 7  // Keep only 7 days instead of 30
```

## Examples

See example configurations:
- `appsettings.Serilog.Development.json` - Development settings
- `appsettings.Serilog.Production.json` - Production settings

Copy these sections to your API project's appsettings files.
