# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

**Project:** HouseLedger
**Created:** October 19, 2025
**Last Updated:** October 23, 2025

---

## 📋 Base Rules

### Documentation Management

**Rule 1: Single Development Plan**
- All development planning MUST be consolidated in ONE file: `development-plan.md`
- The file MUST have a summary table at the top showing:
  - Phase/Task name
  - Status (Not Started / In Progress / Completed)
  - Start Date
  - Completion Date
  - Notes

**Rule 2: Completed Implementation Archive**
- All completed implementation details MUST be moved to: `development-plan-done.md`
- This keeps the main plan file focused on current/future work
- Archive includes:
  - What was built
  - Key decisions made
  - Code samples (if relevant)
  - Problems encountered and solutions

**Rule 3: Delete Redundant Documentation**
- Once consolidated into `development-plan.md` and `development-plan-done.md`:
  - DELETE old plan files to avoid confusion
  - Keep only: architecture docs, mindset docs, and the two plan files

**Rule 4: Ask Before Choosing Implementation Pattern**
- **IMPORTANT**: When creating new services, always ask the user which pattern to use:
  - **Option A: MediatR + CQRS** (Finance service pattern)
  - **Option B: Simple CRUD Services** (Ancillary service pattern)
- Do not assume which pattern to use without asking first

---

## Project Overview

HouseLedger is a modern home finance management application built with .NET 10 and ASP.NET Core, targeting deployment on a QNAP TS-231P NAS (ARM32, 1GB RAM). The application follows Clean Architecture principles with a vertical slice architecture approach, emphasizing simplicity over complexity (based on Rich Hickey's "Simple Made Easy" philosophy).

---

## 🏗️ Architecture

### High-Level Structure

The codebase is organized into three main layers:

1. **Core/** - Shared domain primitives (BaseEntity, AuditableEntity, ValueObject, interfaces)
2. **Services/** - Vertical slices organized by bounded context:
   - **Finance** - Transactions, accounts, balances, banks, cards (uses MediatR + CQRS)
   - **Ancillary** - Currencies, countries, currency conversion rates, suppliers (uses simple CRUD)
   - **Salary** - Salary entries (uses simple CRUD like Ancillary)
3. **BuildingBlocks/** - Reusable infrastructure components:
   - **Logging** - Serilog configuration with console + JSON file output
   - **BackgroundJobs** - Hangfire-based job scheduling with abstractions
   - **Authentication** - JWT Bearer authentication with ASP.NET Core Identity

### Service Architecture Patterns

**Two patterns are used in this codebase:**

#### Pattern A: MediatR + CQRS (Finance Service)
```
HouseLedger.Services.Finance/
├── Finance.Domain/              # Entities, Value Objects
├── Finance.Application/         # Features (CQRS), DTOs, Handlers, Validators
│   └── Features/
│       └── Transactions/
│           └── CreateTransaction/
│               ├── CreateTransactionCommand.cs
│               ├── CreateTransactionHandler.cs
│               └── CreateTransactionValidator.cs
└── Finance.Infrastructure/      # Persistence, DbContext
```

#### Pattern B: Simple CRUD Services (Ancillary & Salary)
```
HouseLedger.Services.Ancillary/
├── Ancillary.Domain/           # Entities
├── Ancillary.Application/      # DTOs, Interfaces, Services, Mapping
│   ├── Contracts/
│   ├── Interfaces/
│   └── Services/               # Direct service implementations
└── Ancillary.Infrastructure/   # Persistence, DbContext, Service Implementations
```

### Key Architectural Decisions

- **Vertical Slices**: Features organized by use case, not by technical layer
- **Two Implementation Patterns**: MediatR+CQRS for complex services, Simple CRUD for simpler ones
- **Single Database**: All services share SQLite database (housledger.db)
- **Monolith Deployment**: All services composed in single API gateway (1GB RAM constraint)
- **Minimal Endpoints**: ASP.NET Core Minimal APIs with endpoint mapping extensions

---

## Development Commands

### Building

```bash
# Build entire solution
dotnet build

# Build specific project
dotnet build src/Api/HouseLedger.Api/HouseLedger.Api.csproj

# Build specific service
dotnet build src/Services/HouseLedger.Services.Finance/HouseLedger.Services.Finance.Domain/HouseLedger.Services.Finance.Domain.csproj
```

### Running

```bash
# Run the API gateway (includes all services)
dotnet run --project src/Api/HouseLedger.Api/HouseLedger.Api.csproj

# Access Swagger UI at http://localhost:5000
# Access Hangfire Dashboard at http://localhost:5000/hangfire
```

### Testing

```bash
# Run all tests
dotnet test

# Run specific test project
dotnet test tests/HouseLedger.Services.Ancillary.UnitTests/HouseLedger.Services.Ancillary.UnitTests.csproj

# Run with detailed output
dotnet test --logger "console;verbosity=detailed"

# Run with code coverage
dotnet test --collect:"XPlat Code Coverage"
```

---

## Service Implementation Patterns

### Pattern A: Adding a Feature with MediatR (Finance-style)

1. **Create Feature Folder**: `Application/Features/{EntityName}/{FeatureName}/`
2. **Define Command/Query**: `{FeatureName}Command.cs` or `{FeatureName}Query.cs`
3. **Implement Handler**: `{FeatureName}Handler.cs` with `IRequestHandler<TRequest, TResponse>`
4. **Add Validator**: `{FeatureName}Validator.cs` with FluentValidation
5. **Create DTO**: Place in `Application/Contracts/{EntityName}/`

### Pattern B: Adding a Feature with Simple CRUD (Ancillary/Salary-style)

1. **Create DTO**: Place in `Application/Contracts/{EntityName}/`
2. **Define Interface**: Add methods to `Application/Interfaces/I{EntityName}Service.cs`
3. **Implement Service**: Create in `Infrastructure/Services/{EntityName}Service.cs`
4. **Register Service**: Add to DI container in Program.cs
5. **Create Endpoints**: Add endpoint mapping methods

### Adding a New Service

**IMPORTANT: Ask user which pattern to use first!**

1. Create folder: `src/Services/HouseLedger.Services.{ServiceName}/`
2. Create three projects: Domain, Application, Infrastructure
3. Define entities in Domain with proper base classes
4. Choose implementation pattern (ask user):
   - **MediatR + CQRS**: For complex business logic, event sourcing
   - **Simple CRUD**: For straightforward data operations
5. Create DbContext in Infrastructure
6. Register DbContext and services in API's Program.cs
7. Create endpoint mapping extension methods
8. Add health check for the new DbContext

### Database Conventions

- All entities inherit from `BaseEntity` (Id) or `AuditableEntity` (adds CreatedDate, LastUpdatedDate, IsActive)
- Entity configurations use Fluent API in separate configuration classes
- Audit fields automatically set in save operations
- Use soft deletes (`IsActive = false`) by default
- Database: SQLite, no migrations (maps to legacy schema)

---

## BuildingBlocks Usage

### Logging (Serilog)

```csharp
// Configure in Program.cs
builder.ConfigureSerilog();

// Use in services
_logger.LogInformation("Transaction created with ID {TransactionId}", transaction.Id);
_logger.LogError(ex, "Failed to create transaction for account {AccountId}", accountId);
```

Logs output to:
- Console (human-readable)
- JSON files in `logs/` (daily rolling, 30-day retention)

### Background Jobs (Hangfire)

```csharp
// Configure in Program.cs
builder.Services.AddHouseLedgerBackgroundJobs(builder.Configuration);
app.UseHouseLedgerBackgroundJobs();
app.RegisterRecurringJobs();

// Create recurring job
public class MyJob : IRecurringJob
{
    public string JobId => "my-job";
    public string CronExpression => "0 2 * * *"; // Daily at 2 AM
    public string Queue => "default";

    public async Task<JobResult> ExecuteAsync(CancellationToken ct)
    {
        return JobResult.Success("Job completed");
    }
}
```

### Authentication (JWT)

```csharp
// Configure in Program.cs
builder.Services.AddJwtAuthentication(builder.Configuration);
app.UseJwtAuthentication();

// Protect endpoints
endpoints.MapGet("/protected", () => "Secret data")
    .RequireAuthorization();
```

---

## 🖥️ Deployment Target: QNAP TS-231P NAS

### Hardware Specifications

**CPU:**
- Model: AnnapurnaLabs Alpine AL-212
- Architecture: ARM Cortex-A15 (32-bit ARM)
- Cores: Dual-core, 1.7 GHz

**Memory:**
- RAM: 1GB DDR3
- Flash: 512 MB

**Storage:**
- 2-bay NAS (user-provided drives)

**Network:**
- Dual Gigabit Ethernet ports

**Performance:**
- Read: Up to 224 MB/s
- Write: Up to 176 MB/s

### Docker Deployment

**Critical:** Must use ARM32-compatible Docker images

```dockerfile
# ❌ WRONG - x64/AMD64 images won't work
FROM mcr.microsoft.com/dotnet/aspnet:8.0

# ✅ CORRECT - ARM32 Linux images
FROM mcr.microsoft.com/dotnet/aspnet:8.0-jammy-arm32v7
```

### Build Strategies

**Recommended: Cross-compile**
```bash
# Build on x64 development machine
dotnet publish -c Release -r linux-arm --self-contained false
```

**Alternative: Multi-arch Docker**
```bash
docker buildx build --platform linux/arm/v7 -t housledger:latest .
```

### Memory Constraints (1GB RAM)

- Container memory limit: 256-512MB max
- Run as monolith, not microservices
- Use lightweight base images (Alpine)
- Monitor memory usage carefully

### Database: SQLite

- No separate database server (saves 100-200MB RAM)
- File-based, perfect for NAS storage
- ARM32 compatible
- Storage path: `/share/HouseLedger/data/housledger.db`

### docker-compose.yml Example

```yaml
version: '3.8'

services:
  housledger:
    image: housledger:arm32v7
    container_name: housledger
    restart: unless-stopped
    ports:
      - "5000:80"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ConnectionStrings__DefaultConnection=Data Source=/data/housledger.db
    volumes:
      - /share/HouseLedger/data:/data
      - /share/HouseLedger/logs:/app/logs
    mem_limit: 512m
    memswap_limit: 512m
```

### Expected Resource Usage

- **API Gateway**: 200-400 MB
- **Hangfire**: 50-100 MB
- **SQLite**: 10-20 MB
- **Total**: ~256-512 MB

---

## 🏗️ Architecture Decision Records

### ADR-007: Monolith Deployment for NAS
**Context:** QNAP TS-231P has only 1GB RAM, ARM32 CPU
**Decision:** Deploy as single monolith application, not microservices
**Rationale:** 1GB RAM insufficient for multiple service containers
**Status:** Approved

### ADR-008: SQLite for Data Persistence
**Context:** Limited memory and ARM32 architecture
**Decision:** Use SQLite file-based database
**Rationale:** No separate database server needed, saves 100-200MB RAM
**Status:** Approved

### ADR-009: ARM32 Docker Images
**Context:** QNAP TS-231P is ARM Cortex-A15 (32-bit)
**Decision:** Use ARM32v7 Docker base images
**Rationale:** x64/AMD64 images will not run on ARM32
**Status:** Approved

### ADR-010: Cross-Compilation Strategy
**Context:** Building on ARM32 with 1GB RAM is slow
**Decision:** Build on x64 development machine, publish for linux-arm
**Rationale:** Faster builds, better developer experience
**Status:** Approved

### ADR-011: Two Service Implementation Patterns
**Context:** Different services have different complexity levels
**Decision:** Use MediatR+CQRS for complex services, Simple CRUD for simple ones
**Rationale:** Don't over-engineer simple CRUD operations; use appropriate complexity
**Status:** Approved

---

## Testing

**Test frameworks:**
- xUnit - Test framework
- Moq - Mocking framework
- FluentAssertions - Assertion library
- Bogus - Test data generation
- EF Core InMemory - In-memory database

**Naming convention:** `MethodName_Scenario_ExpectedResult`

```csharp
[Fact]
public async Task CreateAsync_ValidRequest_ReturnsDto()
{
    // Arrange
    var request = TestDataBuilder.CreateRequest();

    // Act
    var result = await _service.CreateAsync(request);

    // Assert
    result.Should().NotBeNull();
    result.Name.Should().Be(request.Name);
}
```

---

## API Structure

Base URL: `http://localhost:5000`

**Main endpoints:**
- `/` - API information
- `/swagger` - OpenAPI documentation
- `/hangfire` - Background job dashboard
- `/health` - Health checks
- `/api/v1/transactions` - Finance: Transactions
- `/api/v1/accounts` - Finance: Accounts
- `/api/v1/currencies` - Ancillary: Currencies
- `/api/v1/salaries` - Salary: Salaries
- `/api/v1/auth` - Authentication

---

## Philosophy: "Simple Made Easy"

Following Rich Hickey's principles:

- **Un-braided services**: Independent with clear boundaries
- **Vertical slices**: Self-contained features
- **Composing not complecting**: Well-defined contracts
- **Values over state**: Prefer immutability
- **Declarative over imperative**: Use LINQ, FluentValidation

**When making changes:**
- Avoid "complecting" (intertwining) concerns
- Keep components simple (one responsibility)
- Favor composition over inheritance
- Make dependencies explicit
- Choose appropriate complexity level

---

## Key Reminders

1. **Always ask which pattern to use** when creating new services
2. **Always use ARM32 images** - x64 will not work on NAS
3. **Memory is limited** - Keep total usage under 512MB
4. **SQLite is your friend** - No separate database server
5. **Build on x64, deploy to ARM32** - Cross-compile for speed
6. **Monolith, not microservices** - RAM constraints
7. **Test on ARM32 before deploying** - Use Raspberry Pi or QEMU
8. **Database migrations**: NOT used (maps to legacy schema)

---

**References:**
- QNAP TS-231P Specs: https://www.qnap.com/en/product/ts-231p/specs/hardware
- .NET ARM32 Support: https://github.com/dotnet/runtime/blob/main/docs/design/features/arm32-support.md
- Docker ARM32 Images: https://hub.docker.com/u/arm32v7
