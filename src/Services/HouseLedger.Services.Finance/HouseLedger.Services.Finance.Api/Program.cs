using Asp.Versioning;
using FluentValidation;
using HouseLedger.BuildingBlocks.Logging;
using HouseLedger.Services.Finance.Api.Endpoints;
using HouseLedger.Services.Finance.Api.Middleware;
using HouseLedger.Services.Finance.Application.Behaviors;
using HouseLedger.Services.Finance.Application.Interfaces;
using HouseLedger.Services.Finance.Application.Services;
using HouseLedger.Services.Finance.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// ===== SERILOG CONFIGURATION =====
// Configure Serilog using BuildingBlocks.Logging
builder.ConfigureSerilog();

// ===== SERVICES CONFIGURATION =====

// Database Context (SQLite)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Data Source=housledger.db";

builder.Services.AddDbContext<FinanceDbContext>(options =>
    options.UseSqlite(connectionString));

// MediatR with Pipeline Behaviors
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(AccountQueryService).Assembly);
    cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
    cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
});

// FluentValidation
builder.Services.AddValidatorsFromAssembly(typeof(AccountQueryService).Assembly);

// AutoMapper
builder.Services.AddAutoMapper(typeof(AccountQueryService).Assembly);

// Traditional Query Services
builder.Services.AddScoped<IAccountQueryService, AccountQueryService>();
builder.Services.AddScoped<ITransactionQueryService, TransactionQueryService>();

// API Versioning (URL path: /api/v1/)
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = new UrlSegmentApiVersionReader();
});

// Global Exception Handler (RFC 7807 ProblemDetails)
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// CORS Configuration (flexible for later)
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        // TODO: Configure based on deployment needs
        // Development: Allow localhost origins
        // Production: Allow specific domains
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Health Checks (liveness + database)
builder.Services.AddHealthChecks()
    .AddDbContextCheck<FinanceDbContext>(
        name: "database",
        tags: new[] { "db", "sql", "sqlite" });

// OpenAPI/Swagger (Development only)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "HouseLedger Finance API",
        Version = "v1",
        Description = "Finance service for managing transactions, accounts, banks, and balances",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "HouseLedger Team"
        }
    });
});

var app = builder.Build();

// ===== MIDDLEWARE PIPELINE =====

// Exception Handler (must be first)
app.UseExceptionHandler();

// Request/Response Logging (Serilog)
app.UseMiddleware<RequestResponseLoggingMiddleware>();

// Swagger (Development only)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Finance API v1");
        options.RoutePrefix = string.Empty; // Swagger UI at root
    });
}

// CORS
app.UseCors();

// Serilog Request Logging (enriches logs with HTTP context)
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
    Predicate = check => check.Tags.Contains("db") // Readiness check (database)
});

// ===== API ENDPOINTS =====
var apiVersionSet = app.NewApiVersionSet()
    .HasApiVersion(new ApiVersion(1, 0))
    .ReportApiVersions()
    .Build();

// V1 Endpoints - Use explicit version number instead of placeholder
var v1Group = app.MapGroup("/api/v1")
    .WithApiVersionSet(apiVersionSet)
    .HasApiVersion(1, 0);

// Transaction endpoints
v1Group.MapGroup("/transactions")
    .MapTransactionEndpointsV1()
    .WithTags("Transactions");

// Account endpoints
v1Group.MapGroup("/accounts")
    .MapAccountEndpointsV1()
    .WithTags("Accounts");

// Root endpoint (API info)
app.MapGet("/", () => new
{
    Name = "HouseLedger Finance API",
    Version = "v1",
    Status = "Running",
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
    Log.Information("Starting HouseLedger Finance API");
    app.Run();
    Log.Information("HouseLedger Finance API stopped cleanly");
}
catch (Exception ex)
{
    Log.Fatal(ex, "HouseLedger Finance API terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
