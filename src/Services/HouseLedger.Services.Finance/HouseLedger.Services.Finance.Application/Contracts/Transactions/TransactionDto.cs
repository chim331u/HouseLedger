namespace HouseLedger.Services.Finance.Application.Contracts.Transactions;

/// <summary>
/// Data transfer object for Transaction entity.
/// </summary>
public class TransactionDto
{
    public int Id { get; set; }
    public DateTime TransactionDate { get; set; }
    public double Amount { get; set; }
    public string? Description { get; set; }
    public string? UniqueKey { get; set; }
    public int? AccountId { get; set; }
    public string? AccountName { get; set; }

    // Category
    public string? CategoryName { get; set; }
    public bool IsCategoryConfirmed { get; set; }

    // Audit fields
    public DateTime CreatedDate { get; set; }
    public DateTime? LastUpdatedDate { get; set; }
    public bool IsActive { get; set; }
    public string? Note { get; set; }
}
