using Asp.Versioning;
using FluentValidation;
using HouseLedger.Api.Endpoints.Ancillary;
using HouseLedger.Api.Endpoints.Auth;
using HouseLedger.Api.Endpoints.Finance;
using HouseLedger.Api.Endpoints.Salary;
using HouseLedger.Api.Endpoints.HouseThings;
using HouseLedger.Api.Services.Auth;
using HouseLedger.BuildingBlocks.Authentication.Configuration;
using HouseLedger.BuildingBlocks.Authentication.Models;
using HouseLedger.BuildingBlocks.Authentication.Services;
using HouseLedger.BuildingBlocks.BackgroundJobs.Configuration;
using HouseLedger.BuildingBlocks.Logging;
using HouseLedger.Services.Ancillary.Application.Interfaces;
using HouseLedger.Services.Ancillary.Application.Services;
using HouseLedger.Services.Ancillary.Infrastructure.Persistence;
using HouseLedger.Services.Finance.Application.Behaviors;
using HouseLedger.Services.Finance.Application.Interfaces;
using HouseLedger.Services.Finance.Application.Services;
using HouseLedger.Services.Finance.Infrastructure.Persistence;
using HouseLedger.Services.Salary.Application.Interfaces;
using HouseLedger.Services.Salary.Application.Services;
using HouseLedger.Services.Salary.Infrastructure.Persistence;
using HouseLedger.Services.HouseThings.Application.Interfaces;
using HouseLedger.Services.HouseThings.Application.Services;
using HouseLedger.Services.HouseThings.Infrastructure.Persistence;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// ===== SERILOG CONFIGURATION =====
builder.ConfigureSerilog();

// ===== HANGFIRE BACKGROUND JOBS =====
builder.Services.AddHouseLedgerBackgroundJobs(builder.Configuration);

// ===== SERVICES CONFIGURATION =====

// Database Contexts (SQLite - same MM.db for all services)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Data Source=housledger.db";

// Finance DbContext
builder.Services.AddDbContext<FinanceDbContext>(options =>
    options.UseSqlite(connectionString));

// Ancillary DbContext
builder.Services.AddDbContext<AncillaryDbContext>(options =>
    options.UseSqlite(connectionString));

// Identity DbContext (same MM.db database)
builder.Services.AddDbContext<AppIdentityDbContext>(options =>
    options.UseSqlite(connectionString));

// Salary DbContext
builder.Services.AddDbContext<SalaryDbContext>(options =>
    options.UseSqlite(connectionString));

// HouseThings DbContext
builder.Services.AddDbContext<HouseThingsDbContext>(options =>
    options.UseSqlite(connectionString));

// ===== AUTHENTICATION CONFIGURATION =====

// JWT Authentication (uses BuildingBlock)
builder.Services.AddJwtAuthentication(builder.Configuration);

// ASP.NET Core Identity (maps to existing AspNetUsers tables)
builder.Services
    .AddIdentityCore<IdentityUser>(options =>
    {
        // Password requirements (stronger than old system)
        options.Password.RequireDigit = true;
        options.Password.RequiredLength = 8;
        options.Password.RequireNonAlphanumeric = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireLowercase = true;

        // User requirements
        options.SignIn.RequireConfirmedAccount = false;
        options.User.RequireUniqueEmail = true;
    })
    .AddEntityFrameworkStores<AppIdentityDbContext>();

// Authentication Services
builder.Services.AddScoped<ITokenService, JwtTokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();

// MediatR with Pipeline Behaviors (Finance only)
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(AccountQueryService).Assembly); // Finance
    cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
    cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
});

// FluentValidation (Finance only - Ancillary and Salary use simple validation)
builder.Services.AddValidatorsFromAssembly(typeof(AccountQueryService).Assembly);

// AutoMapper (Finance + Ancillary + Salary + HouseThings)
builder.Services.AddAutoMapper(
    typeof(AccountQueryService).Assembly,           // Finance mappings
    typeof(CurrencyQueryService).Assembly,          // Ancillary mappings
    typeof(SalaryQueryService).Assembly,            // Salary mappings
    typeof(RoomQueryService).Assembly);             // HouseThings mappings

// Finance Query Services
builder.Services.AddScoped<IAccountQueryService, AccountQueryService>();
builder.Services.AddScoped<ITransactionQueryService, TransactionQueryService>();
builder.Services.AddScoped<IBankQueryService, BankQueryService>();
builder.Services.AddScoped<IBalanceQueryService, BalanceQueryService>();

// Finance Command Services
builder.Services.AddScoped<IAccountCommandService, AccountCommandService>();
builder.Services.AddScoped<IBankCommandService, BankCommandService>();
builder.Services.AddScoped<IBalanceCommandService, BalanceCommandService>();

// Ancillary Query Services
builder.Services.AddScoped<ICurrencyQueryService, CurrencyQueryService>();
builder.Services.AddScoped<ICountryQueryService, CountryQueryService>();
builder.Services.AddScoped<ICurrencyConversionRateQueryService, CurrencyConversionRateQueryService>();
builder.Services.AddScoped<ISupplierQueryService, SupplierQueryService>();
builder.Services.AddScoped<IServiceUserQueryService, ServiceUserQueryService>();

// Ancillary Command Services
builder.Services.AddScoped<ICurrencyCommandService, CurrencyCommandService>();
builder.Services.AddScoped<ICountryCommandService, CountryCommandService>();
builder.Services.AddScoped<ICurrencyConversionRateCommandService, CurrencyConversionRateCommandService>();
builder.Services.AddScoped<ISupplierCommandService, SupplierCommandService>();
builder.Services.AddScoped<IServiceUserCommandService, ServiceUserCommandService>();

// Salary Query and Command Services
builder.Services.AddScoped<ISalaryQueryService, SalaryQueryService>();
builder.Services.AddScoped<ISalaryCommandService, SalaryCommandService>();

// HouseThings Query Services
builder.Services.AddScoped<IRoomQueryService, RoomQueryService>();
builder.Services.AddScoped<IHouseThingQueryService, HouseThingQueryService>();

// HouseThings Command Services
builder.Services.AddScoped<IRoomCommandService, RoomCommandService>();
builder.Services.AddScoped<IHouseThingCommandService, HouseThingCommandService>();

// API Versioning (URL path: /api/v1/)
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = new UrlSegmentApiVersionReader();
});

// CORS Configuration
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Health Checks (all databases)
builder.Services.AddHealthChecks()
    .AddDbContextCheck<FinanceDbContext>(
        name: "finance-database",
        tags: new[] { "db", "sql", "sqlite", "finance" })
    .AddDbContextCheck<AncillaryDbContext>(
        name: "ancillary-database",
        tags: new[] { "db", "sql", "sqlite", "ancillary" })
    .AddDbContextCheck<AppIdentityDbContext>(
        name: "identity-database",
        tags: new[] { "db", "sql", "sqlite", "identity" })
    .AddDbContextCheck<SalaryDbContext>(
        name: "salary-database",
        tags: new[] { "db", "sql", "sqlite", "salary" })
    .AddDbContextCheck<HouseThingsDbContext>(
        name: "housethings-database",
        tags: new[] { "db", "sql", "sqlite", "housethings" });

// OpenAPI/Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "HouseLedger API Gateway",
        Version = "v1",
        Description = "Unified API Gateway for all HouseLedger services (Finance, Ancillary, Authentication, etc.)",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "HouseLedger Team"
        }
    });

    // JWT Bearer Authentication
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Enter your JWT token in the format: {your token}"
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// ===== MIDDLEWARE PIPELINE =====

// Swagger (Development only)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "HouseLedger API v1");
        options.RoutePrefix = string.Empty; // Swagger UI at root
    });
}

// CORS
app.UseCors();

// Authentication & Authorization (MUST be after UseCors and before endpoints)
app.UseJwtAuthentication();

// Hangfire Dashboard
app.UseHouseLedgerBackgroundJobs();

// Serilog Request Logging
app.UseSerilogRequestLogging(options =>
{
    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
    {
        diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
        diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
        diagnosticContext.Set("UserAgent", httpContext.Request.Headers["User-Agent"].ToString());
    };
});

// ===== HEALTH CHECKS =====
app.MapHealthChecks("/health");
app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = _ => false // Basic liveness check (no dependencies)
});
app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("db") // Readiness check (all databases)
});

// ===== API ENDPOINTS =====
var apiVersionSet = app.NewApiVersionSet()
    .HasApiVersion(new ApiVersion(1, 0))
    .ReportApiVersions()
    .Build();

var v1Group = app.MapGroup("/api/v1")
    .WithApiVersionSet(apiVersionSet)
    .HasApiVersion(1, 0);

// ===== FINANCE SERVICE ENDPOINTS =====
v1Group.MapGroup("/transactions")
    .MapTransactionEndpointsV1()
    .WithTags("Finance - Transactions");

v1Group.MapGroup("/accounts")
    .MapAccountEndpointsV1()
    .WithTags("Finance - Accounts");

v1Group.MapGroup("/banks")
    .MapBankEndpointsV1()
    .WithTags("Finance - Banks");

v1Group.MapGroup("/balances")
    .MapBalanceEndpointsV1()
    .WithTags("Finance - Balances");

// ===== ANCILLARY SERVICE ENDPOINTS =====
v1Group.MapGroup("/currencies")
    .MapCurrencyEndpointsV1()
    .WithTags("Ancillary - Currencies");

v1Group.MapGroup("/countries")
    .MapCountryEndpointsV1()
    .WithTags("Ancillary - Countries");

v1Group.MapGroup("/currency-conversion-rates")
    .MapCurrencyConversionRateEndpointsV1()
    .WithTags("Ancillary - Currency Conversion Rates");

v1Group.MapGroup("/suppliers")
    .MapSupplierEndpointsV1()
    .WithTags("Ancillary - Suppliers");

v1Group.MapGroup("/serviceusers")
    .MapServiceUserEndpointsV1()
    .WithTags("Ancillary - Service Users");

// ===== SALARY SERVICE ENDPOINTS =====
v1Group.MapGroup("/salaries")
    .MapSalaryEndpointsV1()
    .WithTags("Salary - Salaries");

// ===== HOUSETHINGS SERVICE ENDPOINTS =====
v1Group.MapGroup("/rooms")
    .MapRoomEndpointsV1()
    .WithTags("HouseThings - Rooms");

v1Group.MapGroup("/housethings")
    .MapHouseThingEndpointsV1()
    .WithTags("HouseThings - House Things");

// ===== AUTHENTICATION ENDPOINTS =====
v1Group.MapGroup("/auth")
    .MapAuthEndpointsV1()
    .WithTags("Authentication");

// Root endpoint (API info)
app.MapGet("/", () => new
{
    Name = "HouseLedger API Gateway",
    Version = "v1",
    Status = "Running",
    Services = new
    {
        Finance = "Transactions, Accounts, Banks, Balances",
        Ancillary = "Currencies, Countries, Currency Conversion Rates, Suppliers, Service Users",
        Salary = "Salary Entries",
        HouseThings = "Rooms, House Things",
        Authentication = "Login, Register, JWT Tokens"
    },
    Endpoints = new
    {
        Swagger = "/swagger",
        Health = "/health",
        HealthLive = "/health/live",
        HealthReady = "/health/ready",
        ApiV1 = "/api/v1",
        Auth = "/api/v1/auth"
    }
})
.WithName("ApiInfo")
.ExcludeFromDescription();

// ===== RUN APPLICATION =====
try
{
    Log.Information("Starting HouseLedger API Gateway");
    app.Run();
    Log.Information("HouseLedger API Gateway stopped cleanly");
}
catch (Exception ex)
{
    Log.Fatal(ex, "HouseLedger API Gateway terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
