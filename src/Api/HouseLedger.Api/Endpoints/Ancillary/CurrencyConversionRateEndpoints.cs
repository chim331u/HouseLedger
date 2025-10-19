using HouseLedger.Services.Ancillary.Application.Contracts.CurrencyConversionRates;
using HouseLedger.Services.Ancillary.Application.Interfaces;

namespace HouseLedger.Api.Endpoints.Ancillary;

/// <summary>
/// Endpoints for Currency Conversion Rate with full CRUD operations.
/// </summary>
public static class CurrencyConversionRateEndpoints
{
    public static RouteGroupBuilder MapCurrencyConversionRateEndpointsV1(this RouteGroupBuilder group)
    {
        // GET /api/v1/currency-conversion-rates - Get all
        group.MapGet("/", async (
            ICurrencyConversionRateQueryService queryService,
            CancellationToken cancellationToken) =>
        {
            var rates = await queryService.GetAllAsync(cancellationToken);
            return Results.Ok(rates);
        })
        .WithName("GetAllCurrencyConversionRates")
        .WithSummary("Get all active currency conversion rates");

        // GET /api/v1/currency-conversion-rates/{id} - Get by ID
        group.MapGet("/{id:int}", async (
            int id,
            ICurrencyConversionRateQueryService queryService,
            CancellationToken cancellationToken) =>
        {
            var rate = await queryService.GetByIdAsync(id, cancellationToken);
            return rate is not null
                ? Results.Ok(rate)
                : Results.NotFound(new { Message = $"Currency conversion rate with ID {id} not found" });
        })
        .WithName("GetCurrencyConversionRateById")
        .WithSummary("Get currency conversion rate by ID");

        // GET /api/v1/currency-conversion-rates/currency/{currencyCode} - Get by currency code
        group.MapGet("/currency/{currencyCode}", async (
            string currencyCode,
            ICurrencyConversionRateQueryService queryService,
            CancellationToken cancellationToken) =>
        {
            var rates = await queryService.GetByCurrencyCodeAsync(currencyCode, cancellationToken);
            return Results.Ok(rates);
        })
        .WithName("GetCurrencyConversionRatesByCurrency")
        .WithSummary("Get conversion rates for a specific currency code");

        // GET /api/v1/currency-conversion-rates/currency/{currencyCode}/date/{date} - Get by currency and date
        group.MapGet("/currency/{currencyCode}/date/{date:datetime}", async (
            string currencyCode,
            DateTime date,
            ICurrencyConversionRateQueryService queryService,
            CancellationToken cancellationToken) =>
        {
            var rate = await queryService.GetByCurrencyAndDateAsync(currencyCode, date, cancellationToken);
            return rate is not null
                ? Results.Ok(rate)
                : Results.NotFound(new { Message = $"Currency conversion rate for {currencyCode} on {date:yyyy-MM-dd} not found" });
        })
        .WithName("GetCurrencyConversionRateByDate")
        .WithSummary("Get conversion rate for a specific currency on a specific date");

        // POST /api/v1/currency-conversion-rates - Create new rate
        group.MapPost("/", async (
            CreateCurrencyConversionRateRequest request,
            ICurrencyConversionRateCommandService commandService,
            CancellationToken cancellationToken) =>
        {
            var rate = await commandService.CreateAsync(request, cancellationToken);
            return Results.Created($"/api/v1/currency-conversion-rates/{rate.Id}", rate);
        })
        .WithName("CreateCurrencyConversionRate")
        .WithSummary("Create a new currency conversion rate");

        // PUT /api/v1/currency-conversion-rates/{id} - Update rate
        group.MapPut("/{id:int}", async (
            int id,
            UpdateCurrencyConversionRateRequest request,
            ICurrencyConversionRateCommandService commandService,
            CancellationToken cancellationToken) =>
        {
            var rate = await commandService.UpdateAsync(id, request, cancellationToken);
            return rate is not null
                ? Results.Ok(rate)
                : Results.NotFound(new { Message = $"Currency conversion rate with ID {id} not found" });
        })
        .WithName("UpdateCurrencyConversionRate")
        .WithSummary("Update an existing currency conversion rate");

        // DELETE /api/v1/currency-conversion-rates/{id}/soft - Soft delete
        group.MapDelete("/{id:int}/soft", async (
            int id,
            ICurrencyConversionRateCommandService commandService,
            CancellationToken cancellationToken) =>
        {
            var success = await commandService.SoftDeleteAsync(id, cancellationToken);
            return success
                ? Results.NoContent()
                : Results.NotFound(new { Message = $"Currency conversion rate with ID {id} not found" });
        })
        .WithName("SoftDeleteCurrencyConversionRate")
        .WithSummary("Soft delete a currency conversion rate (set IsActive = false)");

        // DELETE /api/v1/currency-conversion-rates/{id}/hard - Hard delete
        group.MapDelete("/{id:int}/hard", async (
            int id,
            ICurrencyConversionRateCommandService commandService,
            CancellationToken cancellationToken) =>
        {
            var success = await commandService.HardDeleteAsync(id, cancellationToken);
            return success
                ? Results.NoContent()
                : Results.NotFound(new { Message = $"Currency conversion rate with ID {id} not found" });
        })
        .WithName("HardDeleteCurrencyConversionRate")
        .WithSummary("Hard delete a currency conversion rate (permanent removal from database)");

        return group;
    }
}
