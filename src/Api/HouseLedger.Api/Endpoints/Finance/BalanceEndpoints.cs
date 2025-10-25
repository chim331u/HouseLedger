using Asp.Versioning.Builder;
using HouseLedger.Services.Finance.Application.Contracts.Balances;
using HouseLedger.Services.Finance.Application.Interfaces;

namespace HouseLedger.Api.Endpoints.Finance;

/// <summary>
/// Balance endpoints using Minimal APIs with Traditional Services (simple CRUD).
/// GET /api/v1/balances/{id} - Get balance by ID
/// GET /api/v1/balances - Get all balances
/// GET /api/v1/balances/account/{accountId} - Get balances by account ID
/// POST /api/v1/balances - Create new balance
/// PUT /api/v1/balances/{id} - Update balance
/// DELETE /api/v1/balances/{id}/soft - Soft delete balance
/// DELETE /api/v1/balances/{id}/hard - Hard delete balance
/// </summary>
public static class BalanceEndpoints
{
    public static RouteGroupBuilder MapBalanceEndpointsV1(this RouteGroupBuilder group)
    {
        // GET /api/v1/balances/{id} - Get by ID
        group.MapGet("/{id:int}", async (
            int id,
            IBalanceQueryService queryService,
            CancellationToken cancellationToken) =>
        {
            var balance = await queryService.GetByIdAsync(id, cancellationToken);
            return balance is not null
                ? Results.Ok(balance)
                : Results.NotFound(new { Message = $"Balance with ID {id} not found" });
        })
        .WithName("GetBalanceById")
        .WithSummary("Get balance by ID")
        .Produces<BalanceDto>()
        .ProducesProblem(StatusCodes.Status404NotFound);

        // GET /api/v1/balances - Get all
        group.MapGet("/", async (
            IBalanceQueryService queryService,
            CancellationToken cancellationToken) =>
        {
            var balances = await queryService.GetAllAsync(cancellationToken);
            return Results.Ok(balances);
        })
        .WithName("GetAllBalances")
        .WithSummary("Get all balances")
        .Produces<IEnumerable<BalanceDto>>();

        // GET /api/v1/balances/account/{accountId} - Get by account ID
        group.MapGet("/account/{accountId:int}", async (
            int accountId,
            IBalanceQueryService queryService,
            CancellationToken cancellationToken) =>
        {
            var balances = await queryService.GetByAccountIdAsync(accountId, cancellationToken);
            return Results.Ok(balances);
        })
        .WithName("GetBalancesByAccountId")
        .WithSummary("Get all balances for a specific account")
        .Produces<IEnumerable<BalanceDto>>();

        // POST /api/v1/balances - Create new balance
        group.MapPost("/", async (
            CreateBalanceRequest request,
            IBalanceCommandService commandService,
            CancellationToken cancellationToken) =>
        {
            var balance = await commandService.CreateAsync(request, cancellationToken);
            return Results.Created($"/api/v1/balances/{balance.Id}", balance);
        })
        .WithName("CreateBalance")
        .WithSummary("Create a new balance")
        .Produces<BalanceDto>(StatusCodes.Status201Created);

        // PUT /api/v1/balances/{id} - Update balance
        group.MapPut("/{id:int}", async (
            int id,
            UpdateBalanceRequest request,
            IBalanceCommandService commandService,
            CancellationToken cancellationToken) =>
        {
            var balance = await commandService.UpdateAsync(id, request, cancellationToken);
            return balance is not null
                ? Results.Ok(balance)
                : Results.NotFound(new { Message = $"Balance with ID {id} not found" });
        })
        .WithName("UpdateBalance")
        .WithSummary("Update an existing balance")
        .Produces<BalanceDto>()
        .ProducesProblem(StatusCodes.Status404NotFound);

        // DELETE /api/v1/balances/{id}/soft - Soft delete
        group.MapDelete("/{id:int}/soft", async (
            int id,
            IBalanceCommandService commandService,
            CancellationToken cancellationToken) =>
        {
            var success = await commandService.SoftDeleteAsync(id, cancellationToken);
            return success
                ? Results.NoContent()
                : Results.NotFound(new { Message = $"Balance with ID {id} not found" });
        })
        .WithName("SoftDeleteBalance")
        .WithSummary("Soft delete a balance (set IsActive = false)")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status404NotFound);

        // DELETE /api/v1/balances/{id}/hard - Hard delete
        group.MapDelete("/{id:int}/hard", async (
            int id,
            IBalanceCommandService commandService,
            CancellationToken cancellationToken) =>
        {
            var success = await commandService.HardDeleteAsync(id, cancellationToken);
            return success
                ? Results.NoContent()
                : Results.NotFound(new { Message = $"Balance with ID {id} not found" });
        })
        .WithName("HardDeleteBalance")
        .WithSummary("Hard delete a balance (permanent removal from database)")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status404NotFound);

        return group;
    }
}
