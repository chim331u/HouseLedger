# Phase 7: Finance API Layer - Summary

**Date:** October 19, 2025
**Status:** ✅ Completed

---

## Overview

Phase 7 successfully implements the Finance API layer using **Minimal APIs** with comprehensive production-ready features including API versioning, global exception handling, health checks, and structured logging.

---

## What Was Created

### Project: Finance.Api

**Location:** `src/Services/HouseLedger.Services.Finance/HouseLedger.Services.Finance.Api/`

### Project Structure

```
Finance.Api/
├── Endpoints/                           # Route registration
│   ├── TransactionEndpoints.cs         ✅ 4 transaction endpoints
│   └── AccountEndpoints.cs             ✅ 3 account endpoints
│
├── Middleware/                          # Custom middleware
│   ├── GlobalExceptionHandler.cs       ✅ RFC 7807 ProblemDetails
│   └── RequestResponseLoggingMiddleware.cs  ✅ Timing + structured logs
│
├── Properties/
│   └── launchSettings.json             ✅ Development launch config
│
├── Program.cs                          ✅ Application startup (~207 lines)
├── appsettings.json                    ✅ Production config (port 8080)
├── appsettings.Development.json        ✅ Development config (ports 5000/5001)
├── appsettings.Production.json         ✅ NAS deployment config
├── HouseLedger.Services.Finance.Api.http  ✅ REST client test file
└── HouseLedger.Services.Finance.Api.csproj  ✅ Project file
```

### Dependencies Added

**NuGet Packages:**
- `Asp.Versioning.Http` (8.1.0) - API versioning
- `Microsoft.AspNetCore.Diagnostics.EntityFrameworkCore` (8.0.0) - EF diagnostics
- `Microsoft.Extensions.Diagnostics.HealthChecks.EntityFrameworkCore` (8.0.0) - Database health checks
- `Swashbuckle.AspNetCore` (6.5.0) - Swagger/OpenAPI
- `FluentValidation.DependencyInjectionExtensions` (11.11.0) - DI integration

**Project References:**
- `Finance.Application` - Application layer (MediatR handlers, Services, DTOs)
- `Finance.Infrastructure` - Infrastructure layer (DbContext, EF Core)
- `BuildingBlocks.Logging` - Serilog configuration

---

## Files Created (10 total)

### Endpoints (2 files)

#### 1. TransactionEndpoints.cs
Maps 4 transaction endpoints to URL pattern `/api/v1/transactions`:

**POST /api/v1/transactions** - Create transaction
- Uses MediatR `CreateTransactionCommand`
- Automatic validation via `ValidationBehavior`
- Returns 201 Created with location header

**GET /api/v1/transactions/{id}** - Get by ID
- Uses `ITransactionQueryService`
- Returns 200 OK or 404 Not Found

**GET /api/v1/transactions/account/{accountId}** - Get by account (paged)
- Optional query parameters: `fromDate`, `toDate`, `page`, `pageSize`
- Returns `PagedResult<TransactionDto>`
- Default page size: 50 (max 100)

**GET /api/v1/transactions/recent** - Get recent transactions (paged)
- Query parameters: `page`, `pageSize`
- Returns `PagedResult<TransactionDto>`
- Sorted by TransactionDate descending

#### 2. AccountEndpoints.cs
Maps 3 account endpoints to URL pattern `/api/v1/accounts`:

**GET /api/v1/accounts/{id}** - Get by ID
- Uses `IAccountQueryService`
- Returns 200 OK or 404 Not Found

**GET /api/v1/accounts** - Get all accounts
- Returns all active accounts
- Includes bank name from navigation property

**GET /api/v1/accounts/bank/{bankId}** - Get by bank
- Returns all accounts for specified bank
- Includes bank name

### Middleware (2 files)

#### 3. GlobalExceptionHandler.cs
Implements `IExceptionHandler` for centralized exception handling:

**Handles:**
- `ValidationException` (FluentValidation) → `ValidationProblemDetails` (400)
  - Groups errors by property name
  - Returns structured validation errors
- `InvalidOperationException` → `ProblemDetails` (400)
  - Business logic errors
- All other exceptions → `ProblemDetails` (500)
  - Internal server errors

**RFC 7807 Format:**
```json
{
  "type": "https://httpstatuses.com/400",
  "title": "Validation Error",
  "status": 400,
  "detail": "One or more validation errors occurred.",
  "instance": "/api/v1/transactions",
  "errors": {
    "Amount": ["Amount cannot be zero"],
    "AccountId": ["Valid account ID is required"]
  }
}
```

#### 4. RequestResponseLoggingMiddleware.cs
Custom middleware for request/response logging:

**Features:**
- Logs request start (Method + Path)
- Measures execution time with Stopwatch
- Logs response completion with status code and timing
- Different log levels based on status code (Warning for 4xx/5xx)
- Logs exceptions with timing

**Example Log:**
```
[INFO] HTTP POST /api/v1/transactions started
[INFO] HTTP POST /api/v1/transactions responded 201 in 45ms
```

### Configuration (4 files)

#### 5. Program.cs (~207 lines)
Comprehensive application startup configuration:

**Services Configuration:**
```csharp
// Serilog
builder.ConfigureSerilog();

// Database
builder.Services.AddDbContext<FinanceDbContext>(...)

// MediatR with behaviors
builder.Services.AddMediatR(cfg => {
    cfg.RegisterServicesFromAssembly(typeof(AccountQueryService).Assembly);
    cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
    cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
});

// FluentValidation, AutoMapper
builder.Services.AddValidatorsFromAssembly(...);
builder.Services.AddAutoMapper(...);

// Traditional Services
builder.Services.AddScoped<IAccountQueryService, AccountQueryService>();
builder.Services.AddScoped<ITransactionQueryService, TransactionQueryService>();

// API Versioning
builder.Services.AddApiVersioning(...);

// Exception Handling
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

// CORS
builder.Services.AddCors(...);

// Health Checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<FinanceDbContext>(...);

// Swagger
builder.Services.AddSwaggerGen(...);
```

**Middleware Pipeline:**
```csharp
app.UseExceptionHandler();                    // 1. Global exception handler (first)
app.UseMiddleware<RequestResponseLoggingMiddleware>();  // 2. Request/response logging
app.UseSwagger();                             // 3. Swagger (Development only)
app.UseSwaggerUI();
app.UseCors();                                // 4. CORS
app.UseSerilogRequestLogging(...);           // 5. Serilog enrichment
app.MapHealthChecks("/health");              // 6. Health checks
app.MapHealthChecks("/health/live", ...);
app.MapHealthChecks("/health/ready", ...);
// 7. API endpoints
```

**Endpoint Registration:**
```csharp
var versionSet = app.NewApiVersionSet()
    .HasApiVersion(new ApiVersion(1, 0))
    .Build();

var v1Group = app.MapGroup("/api/v{version:apiVersion}")
    .WithApiVersionSet(versionSet);

v1Group.MapGroup("/transactions")
    .MapTransactionEndpointsV1()
    .WithTags("Transactions");

v1Group.MapGroup("/accounts")
    .MapAccountEndpointsV1()
    .WithTags("Accounts");
```

#### 6. appsettings.json (Production)
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=housledger.db"
  },
  "AllowedHosts": "*",
  "Kestrel": {
    "Endpoints": {
      "Http": {
        "Url": "http://0.0.0.0:8080"
      }
    }
  }
}
```

#### 7. appsettings.Development.json
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=housledger-dev.db"
  },
  "Kestrel": {
    "Endpoints": {
      "Http": {
        "Url": "http://localhost:5000"
      },
      "Https": {
        "Url": "https://localhost:5001"
      }
    }
  }
}
```

#### 8. appsettings.Production.json (NAS Deployment)
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=/data/housledger.db"
  },
  "Kestrel": {
    "Endpoints": {
      "Http": {
        "Url": "http://0.0.0.0:8080"
      }
    }
  }
}
```

---

## Key Design Patterns

### 1. Minimal APIs

**Why Minimal APIs instead of Controllers?**
- Modern .NET approach (introduced in .NET 6+)
- Less ceremony than MVC controllers
- Better performance (no controller instantiation)
- Lambda-based syntax more concise
- Easier to understand for simple APIs

**Example:**
```csharp
// Minimal API (concise)
group.MapPost("/", async (CreateTransactionCommand command, IMediator mediator, CancellationToken ct) =>
{
    var result = await mediator.Send(command, ct);
    return Results.Created($"/api/v1/transactions/{result.Id}", result);
});

// vs Controller (more ceremony)
[HttpPost]
public async Task<ActionResult<TransactionDto>> Create(
    [FromBody] CreateTransactionCommand command,
    CancellationToken cancellationToken)
{
    var result = await _mediator.Send(command, cancellationToken);
    return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
}
```

### 2. API Versioning (URL Path)

**Pattern:** `/api/v{version}/resource`

**Why URL Path Versioning?**
- Most visible and explicit
- Recommended by Microsoft
- Easy to test in browser
- Clear in documentation
- Better than header or query string versioning

**Configuration:**
```csharp
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = new UrlSegmentApiVersionReader();
});
```

### 3. RFC 7807 ProblemDetails

**Standard error response format:**
```json
{
  "type": "https://httpstatuses.com/400",
  "title": "Bad Request",
  "status": 400,
  "detail": "Account 5 not found or inactive",
  "instance": "/api/v1/transactions"
}
```

**Benefits:**
- Industry standard
- Structured JSON format
- Built into ASP.NET Core
- Consistent across all endpoints
- Supports validation errors

### 4. Health Checks (Kubernetes-style)

**Three endpoints:**

**/health** - Overall health
- Runs all health checks
- Returns "Healthy" if all checks pass

**/health/live** - Liveness probe
- No dependencies (always healthy if API is running)
- Used by Kubernetes to know if container should be restarted

**/health/ready** - Readiness probe
- Checks database connectivity
- Used by Kubernetes to know if container should receive traffic

**Configuration:**
```csharp
builder.Services.AddHealthChecks()
    .AddDbContextCheck<FinanceDbContext>(
        name: "database",
        tags: new[] { "db", "sql", "sqlite" });

// Liveness (no dependencies)
app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = _ => false
});

// Readiness (database only)
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("db")
});
```

### 5. Hybrid MediatR Usage in API

**Complex Operations → MediatR:**
```csharp
// POST /api/v1/transactions - Create (complex)
group.MapPost("/", async (CreateTransactionCommand command, IMediator mediator, ...) =>
{
    var result = await mediator.Send(command, ...);
    return Results.Created(...);
});
```

**Simple Queries → Traditional Services:**
```csharp
// GET /api/v1/accounts/{id} - Get by ID (simple)
group.MapGet("/{id:int}", async (int id, IAccountQueryService queryService, ...) =>
{
    var account = await queryService.GetByIdAsync(id, ...);
    return account is not null ? Results.Ok(account) : Results.NotFound(...);
});
```

### 6. Serilog Integration

**BuildingBlocks.Logging usage:**
```csharp
// Simple one-liner
builder.ConfigureSerilog();
```

**Request logging with enrichment:**
```csharp
app.UseSerilogRequestLogging(options =>
{
    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
    {
        diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
        diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
        diagnosticContext.Set("UserAgent", httpContext.Request.Headers["User-Agent"].ToString());
    };
});
```

---

## API Endpoints Reference

### Transaction Endpoints

| Method | Endpoint | Handler | Response |
|--------|----------|---------|----------|
| POST | `/api/v1/transactions` | MediatR Command | 201 Created |
| GET | `/api/v1/transactions/{id}` | Query Service | 200 OK / 404 Not Found |
| GET | `/api/v1/transactions/account/{accountId}` | Query Service | 200 OK (paged) |
| GET | `/api/v1/transactions/recent` | Query Service | 200 OK (paged) |

### Account Endpoints

| Method | Endpoint | Handler | Response |
|--------|----------|---------|----------|
| GET | `/api/v1/accounts/{id}` | Query Service | 200 OK / 404 Not Found |
| GET | `/api/v1/accounts` | Query Service | 200 OK |
| GET | `/api/v1/accounts/bank/{bankId}` | Query Service | 200 OK |

### Utility Endpoints

| Method | Endpoint | Response |
|--------|----------|----------|
| GET | `/` | API info JSON |
| GET | `/health` | Health status (all) |
| GET | `/health/live` | Liveness status |
| GET | `/health/ready` | Readiness status (DB) |
| GET | `/swagger` | Swagger UI (Dev only) |

---

## Environment Configuration

### Development
- **HTTP:** localhost:5000
- **HTTPS:** localhost:5001
- **Database:** housledger-dev.db (local file)
- **Swagger:** Enabled at `/swagger`
- **Logging:** Console (text) + File (JSON)

### Production (QNAP NAS)
- **HTTP:** 0.0.0.0:8080 (Docker container)
- **Database:** /data/housledger.db (Docker volume mount)
- **Swagger:** Disabled
- **Logging:** File only (JSON)

---

## Testing the API

### Using Swagger UI (Development)
```bash
dotnet run --project src/Services/HouseLedger.Services.Finance/HouseLedger.Services.Finance.Api/
# Navigate to: http://localhost:5000
```

### Using .http File
```http
### Get API Info
GET http://localhost:5000/

### Create Transaction
POST http://localhost:5000/api/v1/transactions
Content-Type: application/json

{
  "transactionDate": "2025-10-19",
  "amount": 50.0,
  "description": "Grocery shopping",
  "accountId": 1,
  "categoryName": "SPESA",
  "isCategoryConfirmed": true
}

### Get Transaction by ID
GET http://localhost:5000/api/v1/transactions/1

### Get Recent Transactions
GET http://localhost:5000/api/v1/transactions/recent?page=1&pageSize=20
```

### Using curl
```bash
# Get API info
curl http://localhost:5000/

# Health checks
curl http://localhost:5000/health
curl http://localhost:5000/health/live
curl http://localhost:5000/health/ready

# Get all accounts
curl http://localhost:5000/api/v1/accounts

# Get account by ID
curl http://localhost:5000/api/v1/accounts/1

# Get recent transactions
curl "http://localhost:5000/api/v1/transactions/recent?page=1&pageSize=10"
```

---

## Benefits Achieved

### ✅ Production-Ready Features

**RFC 7807 ProblemDetails:**
- Standardized error responses
- Validation errors properly structured
- Consistent across all endpoints

**Health Checks:**
- Kubernetes-style liveness/readiness probes
- Database connectivity monitoring
- Ready for container orchestration

**Structured Logging:**
- Request/response timing
- HTTP context enrichment
- JSON logs for analysis

### ✅ Modern API Design

**Minimal APIs:**
- Less boilerplate code
- Better performance
- Easier to maintain

**API Versioning:**
- Future-proof URL structure
- Explicit version in path
- Easy to deprecate old versions

### ✅ Developer Experience

**Swagger UI:**
- Interactive API documentation
- Test endpoints in browser
- Auto-generated from code

**Environment Configuration:**
- Development-friendly defaults (localhost:5000)
- Production-ready settings (port 8080, Docker volumes)

### ✅ Simple Made Easy

**Clear Separation:**
- Endpoints in separate files
- Middleware in dedicated classes
- Configuration centralized in Program.cs

**Hybrid Approach:**
- Complex operations → MediatR (automatic validation, logging)
- Simple queries → Services (straightforward, no ceremony)

---

## Next Steps

**Phase 8: Finance Service - Features**

Will implement additional business logic features:
1. Update transaction (MediatR)
2. Delete transaction (MediatR)
3. Upload transactions from CSV (MediatR)
4. Categorize transaction with ML (MediatR)
5. Get account balance history (Service)
6. Get transactions by category (Service)

---

## Build Verification

✅ **Finance.Api builds successfully**
```bash
dotnet build src/Services/HouseLedger.Services.Finance/HouseLedger.Services.Finance.Api/
# Result: Compilazione completata. Avvisi: 0, Errori: 0
```

---

## Files Summary

| Category | Files | Lines (approx) |
|----------|-------|----------------|
| Endpoints | 2 | 180 |
| Middleware | 2 | 150 |
| Configuration | 5 | 300 |
| **Total** | **9** | **~630** |

---

**Status:** ✅ Phase 7 Complete
**Ready For:** Phase 8 - Finance Service Features
**Build:** ✅ Success (0 warnings, 0 errors)
