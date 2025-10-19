# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

This is a personal finance management application called "MM" (Money Manager) consisting of three interconnected .NET projects:

1. **mm_restapi** - ASP.NET Core 6.0 REST API backend
2. **mm_web** - Blazor WebAssembly frontend
3. **mm_app** - .NET MAUI cross-platform mobile application (Android, iOS, macOS Catalyst, Windows)

All three projects share similar domain models and service interfaces, communicating through the REST API.

## Build and Run Commands

### REST API (mm_restapi)
```bash
# Navigate to the REST API project
cd mm_restapi/MM_RESTAPI

# Build
dotnet build MM_RESTAPI.csproj

# Run
dotnet run --project MM_RESTAPI.csproj

# Run with hot reload
dotnet watch run

# Create migration
dotnet ef migrations add MigrationName --project MM_RESTAPI.csproj

# Apply migrations
dotnet ef database update --project MM_RESTAPI.csproj
```

### Blazor Web (mm_web)
```bash
# Navigate to the web project
cd mm_web/MM_WEB

# Build
dotnet build MM_WEB.csproj

# Run
dotnet run --project MM_WEB.csproj

# Run with hot reload
dotnet watch run
```

### MAUI App (mm_app)
```bash
# Navigate to the app project
cd mm_app

# Build
dotnet build MM_App.csproj

# Run on Android
dotnet build -t:Run -f net8.0-android

# Run on iOS
dotnet build -t:Run -f net8.0-ios

# Run on macOS
dotnet build -t:Run -f net8.0-maccatalyst

# Run on Windows
dotnet build -t:Run -f net8.0-windows10.0.19041.0
```

## Architecture Overview

### Backend Architecture (mm_restapi)

The REST API follows a clean layered architecture:

- **Controllers/** - API endpoints organized by domain (Access, Account, Balance, BankAccount, Bill, Transaction, Salary, HouseThings, IdentityAccess, Statistic, Currency, Country, Supplier)
- **Services/** - Business logic implementation
- **Interfaces/** - Service contracts
- **Data/** - Entity models and DbContext organized by domain folders
- **Migrations/** - EF Core database migrations

**Key Technologies:**
- Entity Framework Core 6.0 with SQLite
- ASP.NET Identity for user management
- JWT Bearer authentication
- Serilog for logging
- Swagger/OpenAPI for API documentation
- ML.NET for transaction categorization (`MlTransactionService`)

**Database Context:** `UsersContext` in `mm_restapi/MM_RESTAPI/Data/UsersContext.cs` contains all DbSets and entity configurations.

**Authentication:** JWT tokens configured in `Program.cs` with credentials in `appsettings.json` under `Jwt` section. Token generation handled by `TokenService`.

**CORS:** Configured to allow requests from web frontend at localhost:7013 and production domain.

### Frontend Architecture (mm_web)

Blazor WebAssembly SPA with component-based architecture:

- **Pages/** - Main pages organized by feature (Access, Anchillary, Balance, BankAccount, Bill, HouseThings, IdentityAccess, Salary, Test, Transaction)
- **Services/** - API client implementations matching backend interfaces
- **Interfaces/** - Service contracts (mirror backend interfaces)
- **Data/** - DTOs and view models organized by domain
- **Shared/** - Reusable Razor components (MainLayout, NotAuthorizePage, Components/)

**Key Technologies:**
- Blazor WebAssembly (standalone)
- Radzen Blazor component library
- Tewr.Blazor.FileReader for file uploads
- PdfPig for PDF processing
- LocalStorage and SessionStorage for client-side state

**Services:** All services inherit from corresponding interface and make HTTP calls to REST API. `LocalStorageAccessor` and `SessionStorageAccessor` handle browser storage.

### Mobile App Architecture (mm_app)

.NET MAUI Blazor Hybrid application sharing UI code with web frontend:

- **Components/Pages/** - Razor pages (Home, About, Access, IdentityAccess, Salary, Settings, Logging)
- **Components/Service/** - Platform-specific service implementations
- **Components/Interface/** - Service contracts
- **Components/Layout/** - App shell and navigation

**Key Technologies:**
- .NET MAUI 8.0 targeting Android, iOS, macOS Catalyst, Windows
- Blazor Hybrid (WebView)
- Radzen Blazor components
- Plugin.Fingerprint for biometric authentication
- Serilog with file logging to cache directory
- `HttpsClientHandlerService` for handling SSL certificates on mobile

**Platform-Specific:** Code in `Platforms/` folders for Android, iOS, Windows, macOS Catalyst initialization.

## Shared Domain Model

The three projects share similar domain concepts:

**Financial Core:**
- Banks, Accounts, Cards (BankAccount domain)
- Transactions with ML-based categorization
- Balances tracking account states over time
- Multi-currency support with conversion rates
- Salaries and income tracking

**Bills Management:**
- Suppliers
- Bills with PDF storage
- ReadInBill for OCR/parsing

**Household:**
- HouseThings (inventory)
- HouseThingsRooms (room organization)

**Identity & Access:**
- ISA_Accounts (Identity Service Accounts)
- ISA_PasswordsOld (legacy password storage)
- ServiceConfigs (external service configuration)
- ServiceUser (service account mappings)

**Statistics:**
- Dashboard aggregations
- Balance charts and trends
- Salary statistics
- Spending analysis

## Service Layer Pattern

All three projects follow a consistent service pattern:

1. **Interface Definition** - `I{Domain}Service` in Interfaces/ folder
2. **Implementation** - `{Domain}Service` in Services/ folder implementing the interface
3. **Dependency Injection** - Registered in Program.cs/MauiProgram.cs with scoped lifetime

Common services across projects:
- `IAccessServices` - Authentication and user management
- `IUtilityServices` - Shared utilities
- `IAnchillaryService` - Supporting data (countries, currencies, etc.)
- `IBankAccountService` - Bank account operations
- `IBalanceService` - Balance tracking
- `ITransactionService` - Transaction CRUD
- `ISalaryService` - Salary management
- `IIdentityAccessService` - Identity and access management
- `IHouseThingsService` - Household inventory
- `IStatisticService` - Analytics and reporting
- `IBillService` - Bill management

**Backend-specific:** `IMlTransactionService` for machine learning transaction categorization

## Configuration

### REST API Configuration (appsettings.json)
- **ConnectionStrings.SqlLiteConnectionFileName** - SQLite database path (default: `/data/MM.db`)
- **Jwt** - JWT authentication settings (Issuer, Audience, Key, TokenDuration)
- **TransactionMlSettings** - ML model paths and training data
- **Folders.BillPath** - Bill PDF storage location
- **Serilog** - Logging configuration with file and console sinks

### Web/App Configuration
Frontend applications read API base URL from their respective appsettings.json files in wwwroot/

## Development Notes

### Database Migrations
The REST API uses Entity Framework Core Code First. When modifying entity models:
1. Update the model in `mm_restapi/MM_RESTAPI/Data/`
2. Create migration: `dotnet ef migrations add MigrationName`
3. Review generated migration code
4. Apply: `dotnet ef database update`

Migrations run automatically on application startup via `db.Database.Migrate()` in Program.cs.

### Adding New Endpoints
When adding new API endpoints:
1. Create entity model in `Data/{Domain}/`
2. Add DbSet to `UsersContext.cs`
3. Create interface in `Interfaces/I{Domain}Service.cs`
4. Implement service in `Services/{Domain}Service.cs`
5. Register service in `Program.cs`
6. Create controller in `Controllers/{Domain}Controller.cs`
7. Mirror interface and implementation in mm_web and mm_app projects

### Shared Code Considerations
The web and mobile app share similar UI patterns. When making changes to Blazor components or page logic, consider if the change should be applied to both projects.

### Security Notes
- JWT tokens have configurable expiration (default 30 minutes)
- API uses ASP.NET Identity with relaxed password requirements (minimum 6 chars)
- CORS configured for specific origins
- User secrets recommended for sensitive configuration (see UserSecretsId in csproj)
- Mobile app includes SSL certificate handling via `IHttpsClientHandlerService`

### Machine Learning
Transaction categorization uses ML.NET with training data in CSV format. Model training and prediction handled by `MlTransactionService`. Training data location configured in `TransactionMlSettings.MlDir`.

## Project Structure Pattern

Each project follows consistent folder organization:
```
{project}/
├── Controllers/ or Pages/      # API endpoints or UI pages
├── Services/                   # Business logic implementations
├── Interfaces/                 # Service contracts
├── Data/                       # Entities, DTOs, ViewModels
│   ├── {Domain}/              # Organized by business domain
├── Program.cs                 # Application entry point and DI setup
└── appsettings.json           # Configuration
```

## Documentation

All documentation files are stored in the `docs/` folder:
- **docs/claude.md** - This file (guidance for Claude Code)
- **docs/simple-mindset.md** - "Simple Made Easy" philosophy guide
- **docs/migration-plan.md** - .NET 8 migration with API wrapper pattern
- **docs/architecture-vertical-slices.md** - Initial vertical slice exploration (reference)
- **docs/architecture-final.md** - **FINAL** architecture with flat service structure

When creating new documentation, always place it in the `docs/` folder.

**Note:** Use `docs/architecture-final.md` as the **AUTHORITATIVE** architecture reference for the .NET 8 migration.

## Architecture Summary

The new .NET 8 architecture follows these simple principles:

1. **MM.Domain** - ALL entities in one flat folder (no subfolders)
2. **MM.Contracts** - ALL DTOs in one flat folder (shared by API, Web, Mobile)
3. **MM.Infrastructure** - ALL data access & external services (DbContext, Repositories, SignalR, Email, ML, Events)
4. **MM.Services/{Domain}/** - Business logic, one folder per service (Finance, Bills, Notifications, etc.)
   - Each service folder contains: Interface, Service implementation, AutoMapper profile
   - Services are **independent** and work by using Domain, Contracts, and Infrastructure
5. **MM.Api** - ASP.NET Core Web API with controllers

**Key insight:** Services like Notifications can use SignalR, Email, and Events from Infrastructure to work independently from other services (like Finance or Bills) while still responding to their events.
