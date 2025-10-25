namespace HouseLedger.Services.Finance.Application.Contracts.Balances;

/// <summary>
/// Request object for updating an existing Balance.
/// </summary>
public record UpdateBalanceRequest
{
    public double Amount { get; init; }
    public DateTime BalanceDate { get; init; }
    public int? AccountId { get; init; }
    public string? Note { get; init; }
}
