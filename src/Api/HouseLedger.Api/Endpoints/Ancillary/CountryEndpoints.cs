using Asp.Versioning.Builder;
using HouseLedger.Services.Ancillary.Application.Contracts.Countries;
using HouseLedger.Services.Ancillary.Application.Interfaces;

namespace HouseLedger.Api.Endpoints.Ancillary;

/// <summary>
/// Country endpoints with full CRUD operations.
/// </summary>
public static class CountryEndpoints
{
    public static RouteGroupBuilder MapCountryEndpointsV1(this RouteGroupBuilder group)
    {
        // GET /api/v1/countries - Get all
        group.MapGet("/", async (
            ICountryQueryService queryService,
            CancellationToken cancellationToken) =>
        {
            var countries = await queryService.GetAllAsync(cancellationToken);
            return Results.Ok(countries);
        })
        .WithName("GetAllCountries")
        .WithSummary("Get all active countries")
        .WithDescription("Returns all active countries ordered by name");

        // GET /api/v1/countries/{id} - Get by ID
        group.MapGet("/{id:int}", async (
            int id,
            ICountryQueryService queryService,
            CancellationToken cancellationToken) =>
        {
            var country = await queryService.GetByIdAsync(id, cancellationToken);
            return country is not null
                ? Results.Ok(country)
                : Results.NotFound(new { Message = $"Country with ID {id} not found" });
        })
        .WithName("GetCountryById")
        .WithSummary("Get country by ID");

        // GET /api/v1/countries/code/{code} - Get by code
        group.MapGet("/code/{code}", async (
            string code,
            ICountryQueryService queryService,
            CancellationToken cancellationToken) =>
        {
            var country = await queryService.GetByCodeAsync(code, cancellationToken);
            return country is not null
                ? Results.Ok(country)
                : Results.NotFound(new { Message = $"Country with code {code} not found" });
        })
        .WithName("GetCountryByCode")
        .WithSummary("Get country by ISO 3166-1 code (e.g., USA, GBR)");

        // POST /api/v1/countries - Create new country
        group.MapPost("/", async (
            CreateCountryRequest request,
            ICountryCommandService commandService,
            CancellationToken cancellationToken) =>
        {
            var country = await commandService.CreateAsync(request, cancellationToken);
            return Results.Created($"/api/v1/countries/{country.Id}", country);
        })
        .WithName("CreateCountry")
        .WithSummary("Create a new country");

        // PUT /api/v1/countries/{id} - Update country
        group.MapPut("/{id:int}", async (
            int id,
            UpdateCountryRequest request,
            ICountryCommandService commandService,
            CancellationToken cancellationToken) =>
        {
            var country = await commandService.UpdateAsync(id, request, cancellationToken);
            return country is not null
                ? Results.Ok(country)
                : Results.NotFound(new { Message = $"Country with ID {id} not found" });
        })
        .WithName("UpdateCountry")
        .WithSummary("Update an existing country");

        // DELETE /api/v1/countries/{id}/soft - Soft delete (set IsActive = false)
        group.MapDelete("/{id:int}/soft", async (
            int id,
            ICountryCommandService commandService,
            CancellationToken cancellationToken) =>
        {
            var success = await commandService.SoftDeleteAsync(id, cancellationToken);
            return success
                ? Results.NoContent()
                : Results.NotFound(new { Message = $"Country with ID {id} not found" });
        })
        .WithName("SoftDeleteCountry")
        .WithSummary("Soft delete a country (set IsActive = false)");

        // DELETE /api/v1/countries/{id}/hard - Hard delete (permanent removal)
        group.MapDelete("/{id:int}/hard", async (
            int id,
            ICountryCommandService commandService,
            CancellationToken cancellationToken) =>
        {
            var success = await commandService.HardDeleteAsync(id, cancellationToken);
            return success
                ? Results.NoContent()
                : Results.NotFound(new { Message = $"Country with ID {id} not found" });
        })
        .WithName("HardDeleteCountry")
        .WithSummary("Hard delete a country (permanent removal from database)");

        return group;
    }
}
