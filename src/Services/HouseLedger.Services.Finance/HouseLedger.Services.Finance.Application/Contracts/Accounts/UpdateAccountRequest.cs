namespace HouseLedger.Services.Finance.Application.Contracts.Accounts;

/// <summary>
/// Request to update an existing account.
/// </summary>
public record UpdateAccountRequest
{
    public string Name { get; init; } = string.Empty;
    public string? AccountNumber { get; init; }
    public string? Description { get; init; }
    public string? Iban { get; init; }
    public string? Bic { get; init; }
    public string? AccountType { get; init; }
    public int? CurrencyId { get; init; }
    public int? BankId { get; init; }
    public string? Note { get; init; }
}
