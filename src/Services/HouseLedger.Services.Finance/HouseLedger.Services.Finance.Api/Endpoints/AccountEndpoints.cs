using Asp.Versioning.Builder;
using HouseLedger.Services.Finance.Application.Contracts.Accounts;
using HouseLedger.Services.Finance.Application.Interfaces;

namespace HouseLedger.Services.Finance.Api.Endpoints;

/// <summary>
/// Account endpoints using Minimal APIs with Traditional Services (simple CRUD).
/// GET /api/v1/accounts/{id} - Get account by ID
/// GET /api/v1/accounts - Get all accounts
/// GET /api/v1/accounts/bank/{bankId} - Get accounts by bank
/// </summary>
public static class AccountEndpoints
{
    public static RouteGroupBuilder MapAccountEndpointsV1(this RouteGroupBuilder group)
    {
        // GET /api/v1/accounts/{id} - Get by ID
        group.MapGet("/{id:int}", async (
            int id,
            IAccountQueryService queryService,
            CancellationToken cancellationToken) =>
        {
            var account = await queryService.GetByIdAsync(id, cancellationToken);
            return account is not null
                ? Results.Ok(account)
                : Results.NotFound(new { Message = $"Account with ID {id} not found" });
        })
        .WithName("GetAccountById")
        .WithSummary("Get account by ID")
        .Produces<AccountDto>()
        .ProducesProblem(StatusCodes.Status404NotFound);

        // GET /api/v1/accounts - Get all
        group.MapGet("/", async (
            IAccountQueryService queryService,
            CancellationToken cancellationToken) =>
        {
            var accounts = await queryService.GetAllAsync(cancellationToken);
            return Results.Ok(accounts);
        })
        .WithName("GetAllAccounts")
        .WithSummary("Get all accounts")
        .Produces<IEnumerable<AccountDto>>();

        // GET /api/v1/accounts/bank/{bankId} - Get by bank
        group.MapGet("/bank/{bankId:int}", async (
            int bankId,
            IAccountQueryService queryService,
            CancellationToken cancellationToken) =>
        {
            var accounts = await queryService.GetByBankIdAsync(bankId, cancellationToken);
            return Results.Ok(accounts);
        })
        .WithName("GetAccountsByBank")
        .WithSummary("Get accounts by bank ID")
        .Produces<IEnumerable<AccountDto>>();

        return group;
    }
}
