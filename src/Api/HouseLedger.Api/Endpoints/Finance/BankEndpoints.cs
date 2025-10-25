using Asp.Versioning.Builder;
using HouseLedger.Services.Finance.Application.Contracts.Banks;
using HouseLedger.Services.Finance.Application.Interfaces;

namespace HouseLedger.Api.Endpoints.Finance;

/// <summary>
/// Bank endpoints using Minimal APIs with Traditional Services (simple CRUD).
/// GET /api/v1/banks/{id} - Get bank by ID
/// GET /api/v1/banks - Get all banks
/// POST /api/v1/banks - Create new bank
/// PUT /api/v1/banks/{id} - Update bank
/// DELETE /api/v1/banks/{id}/soft - Soft delete bank
/// DELETE /api/v1/banks/{id}/hard - Hard delete bank
/// </summary>
public static class BankEndpoints
{
    public static RouteGroupBuilder MapBankEndpointsV1(this RouteGroupBuilder group)
    {
        // GET /api/v1/banks/{id} - Get by ID
        group.MapGet("/{id:int}", async (
            int id,
            IBankQueryService queryService,
            CancellationToken cancellationToken) =>
        {
            var bank = await queryService.GetByIdAsync(id, cancellationToken);
            return bank is not null
                ? Results.Ok(bank)
                : Results.NotFound(new { Message = $"Bank with ID {id} not found" });
        })
        .WithName("GetBankById")
        .WithSummary("Get bank by ID")
        .Produces<BankDto>()
        .ProducesProblem(StatusCodes.Status404NotFound);

        // GET /api/v1/banks - Get all
        group.MapGet("/", async (
            IBankQueryService queryService,
            CancellationToken cancellationToken) =>
        {
            var banks = await queryService.GetAllAsync(cancellationToken);
            return Results.Ok(banks);
        })
        .WithName("GetAllBanks")
        .WithSummary("Get all banks")
        .Produces<IEnumerable<BankDto>>();

        // POST /api/v1/banks - Create new bank
        group.MapPost("/", async (
            CreateBankRequest request,
            IBankCommandService commandService,
            CancellationToken cancellationToken) =>
        {
            var bank = await commandService.CreateAsync(request, cancellationToken);
            return Results.Created($"/api/v1/banks/{bank.Id}", bank);
        })
        .WithName("CreateBank")
        .WithSummary("Create a new bank")
        .Produces<BankDto>(StatusCodes.Status201Created);

        // PUT /api/v1/banks/{id} - Update bank
        group.MapPut("/{id:int}", async (
            int id,
            UpdateBankRequest request,
            IBankCommandService commandService,
            CancellationToken cancellationToken) =>
        {
            var bank = await commandService.UpdateAsync(id, request, cancellationToken);
            return bank is not null
                ? Results.Ok(bank)
                : Results.NotFound(new { Message = $"Bank with ID {id} not found" });
        })
        .WithName("UpdateBank")
        .WithSummary("Update an existing bank")
        .Produces<BankDto>()
        .ProducesProblem(StatusCodes.Status404NotFound);

        // DELETE /api/v1/banks/{id}/soft - Soft delete
        group.MapDelete("/{id:int}/soft", async (
            int id,
            IBankCommandService commandService,
            CancellationToken cancellationToken) =>
        {
            var success = await commandService.SoftDeleteAsync(id, cancellationToken);
            return success
                ? Results.NoContent()
                : Results.NotFound(new { Message = $"Bank with ID {id} not found" });
        })
        .WithName("SoftDeleteBank")
        .WithSummary("Soft delete a bank (set IsActive = false)")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status404NotFound);

        // DELETE /api/v1/banks/{id}/hard - Hard delete
        group.MapDelete("/{id:int}/hard", async (
            int id,
            IBankCommandService commandService,
            CancellationToken cancellationToken) =>
        {
            var success = await commandService.HardDeleteAsync(id, cancellationToken);
            return success
                ? Results.NoContent()
                : Results.NotFound(new { Message = $"Bank with ID {id} not found" });
        })
        .WithName("HardDeleteBank")
        .WithSummary("Hard delete a bank (permanent removal from database)")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status404NotFound);

        return group;
    }
}
