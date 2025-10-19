namespace HouseLedger.Services.Finance.Application.Contracts.Balances;

/// <summary>
/// Data transfer object for Balance entity.
/// </summary>
public class BalanceDto
{
    public int Id { get; set; }
    public double Amount { get; set; }
    public DateTime BalanceDate { get; set; }
    public int? AccountId { get; set; }
    public string? AccountName { get; set; }

    // Audit fields
    public DateTime CreatedDate { get; set; }
    public DateTime? LastUpdatedDate { get; set; }
    public bool IsActive { get; set; }
    public string? Note { get; set; }
}
