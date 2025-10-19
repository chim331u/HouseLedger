namespace HouseLedger.Services.Finance.Application.Contracts.Accounts;

/// <summary>
/// Data transfer object for Account entity.
/// </summary>
public class AccountDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? AccountNumber { get; set; }
    public string? Description { get; set; }
    public string? Iban { get; set; }
    public string? Bic { get; set; }
    public string? AccountType { get; set; }
    public int? CurrencyId { get; set; }
    public int? BankId { get; set; }
    public string? BankName { get; set; }

    // Audit fields
    public DateTime CreatedDate { get; set; }
    public DateTime? LastUpdatedDate { get; set; }
    public bool IsActive { get; set; }
    public string? Note { get; set; }
}
