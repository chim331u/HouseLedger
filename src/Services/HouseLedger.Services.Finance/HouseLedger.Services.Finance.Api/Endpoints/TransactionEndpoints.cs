using Asp.Versioning;
using Asp.Versioning.Builder;
using HouseLedger.Services.Finance.Application.Contracts.Transactions;
using HouseLedger.Services.Finance.Application.Features.Transactions.CreateTransaction;
using HouseLedger.Services.Finance.Application.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace HouseLedger.Services.Finance.Api.Endpoints;

/// <summary>
/// Transaction endpoints using Minimal APIs.
/// POST /api/v1/transactions - Create transaction (MediatR)
/// GET /api/v1/transactions/{id} - Get transaction by ID (Traditional Service)
/// GET /api/v1/transactions/account/{accountId} - Get transactions by account (Traditional Service, paged)
/// GET /api/v1/transactions/recent - Get recent transactions (Traditional Service, paged)
/// </summary>
public static class TransactionEndpoints
{
    public static RouteGroupBuilder MapTransactionEndpointsV1(this RouteGroupBuilder group)
    {
        // POST /api/v1/transactions - Create transaction
        group.MapPost("/", async (
            [FromBody] CreateTransactionCommand command,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var transaction = await mediator.Send(command, cancellationToken);
            return Results.Created($"/api/v1/transactions/{transaction.Id}", transaction);
        })
        .WithName("CreateTransaction")
        .WithSummary("Create a new transaction")
        .WithDescription("Creates a new transaction with validation and duplicate detection")
        .Produces<TransactionDto>(StatusCodes.Status201Created)
        .ProducesValidationProblem()
        .ProducesProblem(StatusCodes.Status400BadRequest);

        // GET /api/v1/transactions/{id} - Get by ID
        group.MapGet("/{id:int}", async (
            int id,
            ITransactionQueryService queryService,
            CancellationToken cancellationToken) =>
        {
            var transaction = await queryService.GetByIdAsync(id, cancellationToken);
            return transaction is not null
                ? Results.Ok(transaction)
                : Results.NotFound(new { Message = $"Transaction with ID {id} not found" });
        })
        .WithName("GetTransactionById")
        .WithSummary("Get transaction by ID")
        .Produces<TransactionDto>()
        .ProducesProblem(StatusCodes.Status404NotFound);

        // GET /api/v1/transactions/account/{accountId} - Get by account
        group.MapGet("/account/{accountId:int}", async (
            int accountId,
            ITransactionQueryService queryService,
            CancellationToken cancellationToken,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50) =>
        {
            var result = await queryService.GetByAccountIdAsync(
                accountId,
                fromDate,
                toDate,
                page,
                pageSize,
                cancellationToken);

            return Results.Ok(result);
        })
        .WithName("GetTransactionsByAccount")
        .WithSummary("Get transactions by account ID")
        .WithDescription("Returns paged transactions for a specific account with optional date filtering")
        .Produces<Application.Contracts.Common.PagedResult<TransactionDto>>();

        // GET /api/v1/transactions/recent - Get recent transactions
        group.MapGet("/recent", async (
            ITransactionQueryService queryService,
            CancellationToken cancellationToken,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50) =>
        {
            var result = await queryService.GetRecentAsync(page, pageSize, cancellationToken);
            return Results.Ok(result);
        })
        .WithName("GetRecentTransactions")
        .WithSummary("Get recent transactions")
        .WithDescription("Returns paged recent transactions across all accounts")
        .Produces<Application.Contracts.Common.PagedResult<TransactionDto>>();

        return group;
    }
}
