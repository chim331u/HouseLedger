namespace HouseLedger.Services.Finance.Application.Contracts.Balances;

/// <summary>
/// Request object for creating a new Balance.
/// </summary>
public record CreateBalanceRequest
{
    public double Amount { get; init; }
    public DateTime BalanceDate { get; init; }
    public int? AccountId { get; init; }
    public string? Note { get; init; }
}
