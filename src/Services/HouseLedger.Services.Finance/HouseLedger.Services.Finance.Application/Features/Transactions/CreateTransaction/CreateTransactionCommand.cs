using HouseLedger.Services.Finance.Application.Contracts.Transactions;
using MediatR;

namespace HouseLedger.Services.Finance.Application.Features.Transactions.CreateTransaction;

/// <summary>
/// Command to create a new transaction.
/// Uses MediatR because it involves validation, business logic, and potential side effects.
/// </summary>
public class CreateTransactionCommand : IRequest<TransactionDto>
{
    public DateTime TransactionDate { get; set; }
    public double Amount { get; set; }
    public string? Description { get; set; }
    public int AccountId { get; set; }
    public string? CategoryName { get; set; }
    public bool IsCategoryConfirmed { get; set; }
    public string? Note { get; set; }
}
