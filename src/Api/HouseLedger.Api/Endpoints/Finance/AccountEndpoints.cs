using Asp.Versioning.Builder;
using HouseLedger.Services.Finance.Application.Contracts.Accounts;
using HouseLedger.Services.Finance.Application.Interfaces;

namespace HouseLedger.Api.Endpoints.Finance;

/// <summary>
/// Account endpoints using Minimal APIs with Traditional Services (simple CRUD).
/// GET /api/v1/accounts/{id} - Get account by ID
/// GET /api/v1/accounts - Get all accounts
/// GET /api/v1/accounts/bank/{bankId} - Get accounts by bank
/// POST /api/v1/accounts - Create new account
/// PUT /api/v1/accounts/{id} - Update account
/// DELETE /api/v1/accounts/{id}/soft - Soft delete account
/// DELETE /api/v1/accounts/{id}/hard - Hard delete account
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

        // POST /api/v1/accounts - Create new account
        group.MapPost("/", async (
            CreateAccountRequest request,
            IAccountCommandService commandService,
            CancellationToken cancellationToken) =>
        {
            var account = await commandService.CreateAsync(request, cancellationToken);
            return Results.Created($"/api/v1/accounts/{account.Id}", account);
        })
        .WithName("CreateAccount")
        .WithSummary("Create a new account")
        .Produces<AccountDto>(StatusCodes.Status201Created);

        // PUT /api/v1/accounts/{id} - Update account
        group.MapPut("/{id:int}", async (
            int id,
            UpdateAccountRequest request,
            IAccountCommandService commandService,
            CancellationToken cancellationToken) =>
        {
            var account = await commandService.UpdateAsync(id, request, cancellationToken);
            return account is not null
                ? Results.Ok(account)
                : Results.NotFound(new { Message = $"Account with ID {id} not found" });
        })
        .WithName("UpdateAccount")
        .WithSummary("Update an existing account")
        .Produces<AccountDto>()
        .ProducesProblem(StatusCodes.Status404NotFound);

        // DELETE /api/v1/accounts/{id}/soft - Soft delete
        group.MapDelete("/{id:int}/soft", async (
            int id,
            IAccountCommandService commandService,
            CancellationToken cancellationToken) =>
        {
            var success = await commandService.SoftDeleteAsync(id, cancellationToken);
            return success
                ? Results.NoContent()
                : Results.NotFound(new { Message = $"Account with ID {id} not found" });
        })
        .WithName("SoftDeleteAccount")
        .WithSummary("Soft delete an account (set IsActive = false)")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status404NotFound);

        // DELETE /api/v1/accounts/{id}/hard - Hard delete
        group.MapDelete("/{id:int}/hard", async (
            int id,
            IAccountCommandService commandService,
            CancellationToken cancellationToken) =>
        {
            var success = await commandService.HardDeleteAsync(id, cancellationToken);
            return success
                ? Results.NoContent()
                : Results.NotFound(new { Message = $"Account with ID {id} not found" });
        })
        .WithName("HardDeleteAccount")
        .WithSummary("Hard delete an account (permanent removal from database)")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status404NotFound);

        return group;
    }
}
