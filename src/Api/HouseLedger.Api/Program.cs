using Asp.Versioning;
using FluentValidation;
using HouseLedger.Api.Endpoints.Ancillary;
using HouseLedger.Api.Endpoints.Finance;
using HouseLedger.BuildingBlocks.BackgroundJobs.Configuration;
using HouseLedger.BuildingBlocks.Logging;
using HouseLedger.Services.Ancillary.Application.Interfaces;
using HouseLedger.Services.Ancillary.Application.Services;
using HouseLedger.Services.Ancillary.Infrastructure.Persistence;
using HouseLedger.Services.Finance.Application.Behaviors;
using HouseLedger.Services.Finance.Application.Interfaces;
using HouseLedger.Services.Finance.Application.Services;
using HouseLedger.Services.Finance.Infrastructure.Persistence;
using MediatR;
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

// MediatR with Pipeline Behaviors (Finance)
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(AccountQueryService).Assembly);
    cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
    cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
});

// FluentValidation (Finance + Ancillary)
builder.Services.AddValidatorsFromAssembly(typeof(AccountQueryService).Assembly);

// AutoMapper (Finance + Ancillary)
builder.Services.AddAutoMapper(
    typeof(AccountQueryService).Assembly,           // Finance mappings
    typeof(CurrencyQueryService).Assembly);         // Ancillary mappings

// Finance Query Services
builder.Services.AddScoped<IAccountQueryService, AccountQueryService>();
builder.Services.AddScoped<ITransactionQueryService, TransactionQueryService>();

// Ancillary Query Services
builder.Services.AddScoped<ICurrencyQueryService, CurrencyQueryService>();
builder.Services.AddScoped<ICountryQueryService, CountryQueryService>();
builder.Services.AddScoped<ICurrencyConversionRateQueryService, CurrencyConversionRateQueryService>();
builder.Services.AddScoped<ISupplierQueryService, SupplierQueryService>();

// Ancillary Command Services
builder.Services.AddScoped<ICurrencyCommandService, CurrencyCommandService>();
builder.Services.AddScoped<ICountryCommandService, CountryCommandService>();
builder.Services.AddScoped<ICurrencyConversionRateCommandService, CurrencyConversionRateCommandService>();
builder.Services.AddScoped<ISupplierCommandService, SupplierCommandService>();

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

// Health Checks (both databases)
builder.Services.AddHealthChecks()
    .AddDbContextCheck<FinanceDbContext>(
        name: "finance-database",
        tags: new[] { "db", "sql", "sqlite", "finance" })
    .AddDbContextCheck<AncillaryDbContext>(
        name: "ancillary-database",
        tags: new[] { "db", "sql", "sqlite", "ancillary" });

// OpenAPI/Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "HouseLedger API Gateway",
        Version = "v1",
        Description = "Unified API Gateway for all HouseLedger services (Finance, Ancillary, etc.)",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "HouseLedger Team"
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

// Root endpoint (API info)
app.MapGet("/", () => new
{
    Name = "HouseLedger API Gateway",
    Version = "v1",
    Status = "Running",
    Services = new
    {
        Finance = "Transactions, Accounts, Banks, Balances",
        Ancillary = "Currencies, Countries, Currency Conversion Rates, Suppliers"
    },
    Endpoints = new
    {
        Swagger = "/swagger",
        Health = "/health",
        HealthLive = "/health/live",
        HealthReady = "/health/ready",
        ApiV1 = "/api/v1"
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
