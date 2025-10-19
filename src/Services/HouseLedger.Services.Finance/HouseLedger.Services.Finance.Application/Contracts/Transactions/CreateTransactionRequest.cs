namespace HouseLedger.Services.Finance.Application.Contracts.Transactions;

/// <summary>
/// Request to create a new transaction.
/// </summary>
public class CreateTransactionRequest
{
    public DateTime TransactionDate { get; set; }
    public double Amount { get; set; }
    public string? Description { get; set; }
    public int AccountId { get; set; }
    public string? CategoryName { get; set; }
    public bool IsCategoryConfirmed { get; set; }
    public string? Note { get; set; }
}
