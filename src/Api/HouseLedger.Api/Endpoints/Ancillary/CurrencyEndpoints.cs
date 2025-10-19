using Asp.Versioning.Builder;
using HouseLedger.Services.Ancillary.Application.Contracts.Currencies;
using HouseLedger.Services.Ancillary.Application.Interfaces;

namespace HouseLedger.Api.Endpoints.Ancillary;

/// <summary>
/// Currency endpoints with full CRUD operations.
/// </summary>
public static class CurrencyEndpoints
{
    public static RouteGroupBuilder MapCurrencyEndpointsV1(this RouteGroupBuilder group)
    {
        // GET /api/v1/currencies - Get all
        group.MapGet("/", async (
            ICurrencyQueryService queryService,
            CancellationToken cancellationToken) =>
        {
            var currencies = await queryService.GetAllAsync(cancellationToken);
            return Results.Ok(currencies);
        })
        .WithName("GetAllCurrencies")
        .WithSummary("Get all active currencies")
        .WithDescription("Returns all active currencies ordered by ISO code");

        // GET /api/v1/currencies/{id} - Get by ID
        group.MapGet("/{id:int}", async (
            int id,
            ICurrencyQueryService queryService,
            CancellationToken cancellationToken) =>
        {
            var currency = await queryService.GetByIdAsync(id, cancellationToken);
            return currency is not null
                ? Results.Ok(currency)
                : Results.NotFound(new { Message = $"Currency with ID {id} not found" });
        })
        .WithName("GetCurrencyById")
        .WithSummary("Get currency by ID");

        // GET /api/v1/currencies/code/{code} - Get by code
        group.MapGet("/code/{code}", async (
            string code,
            ICurrencyQueryService queryService,
            CancellationToken cancellationToken) =>
        {
            var currency = await queryService.GetByCodeAsync(code, cancellationToken);
            return currency is not null
                ? Results.Ok(currency)
                : Results.NotFound(new { Message = $"Currency with code {code} not found" });
        })
        .WithName("GetCurrencyByCode")
        .WithSummary("Get currency by ISO 4217 code (e.g., USD, EUR)");

        // POST /api/v1/currencies - Create new currency
        group.MapPost("/", async (
            CreateCurrencyRequest request,
            ICurrencyCommandService commandService,
            CancellationToken cancellationToken) =>
        {
            var currency = await commandService.CreateAsync(request, cancellationToken);
            return Results.Created($"/api/v1/currencies/{currency.Id}", currency);
        })
        .WithName("CreateCurrency")
        .WithSummary("Create a new currency");

        // PUT /api/v1/currencies/{id} - Update currency
        group.MapPut("/{id:int}", async (
            int id,
            UpdateCurrencyRequest request,
            ICurrencyCommandService commandService,
            CancellationToken cancellationToken) =>
        {
            var currency = await commandService.UpdateAsync(id, request, cancellationToken);
            return currency is not null
                ? Results.Ok(currency)
                : Results.NotFound(new { Message = $"Currency with ID {id} not found" });
        })
        .WithName("UpdateCurrency")
        .WithSummary("Update an existing currency");

        // DELETE /api/v1/currencies/{id}/soft - Soft delete (set IsActive = false)
        group.MapDelete("/{id:int}/soft", async (
            int id,
            ICurrencyCommandService commandService,
            CancellationToken cancellationToken) =>
        {
            var success = await commandService.SoftDeleteAsync(id, cancellationToken);
            return success
                ? Results.NoContent()
                : Results.NotFound(new { Message = $"Currency with ID {id} not found" });
        })
        .WithName("SoftDeleteCurrency")
        .WithSummary("Soft delete a currency (set IsActive = false)");

        // DELETE /api/v1/currencies/{id}/hard - Hard delete (permanent removal)
        group.MapDelete("/{id:int}/hard", async (
            int id,
            ICurrencyCommandService commandService,
            CancellationToken cancellationToken) =>
        {
            var success = await commandService.HardDeleteAsync(id, cancellationToken);
            return success
                ? Results.NoContent()
                : Results.NotFound(new { Message = $"Currency with ID {id} not found" });
        })
        .WithName("HardDeleteCurrency")
        .WithSummary("Hard delete a currency (permanent removal from database)");

        return group;
    }
}
