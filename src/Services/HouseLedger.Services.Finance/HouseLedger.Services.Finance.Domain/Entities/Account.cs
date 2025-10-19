using HouseLedger.Core.Domain.Common;

namespace HouseLedger.Services.Finance.Domain.Entities;

/// <summary>
/// Represents a bank account.
/// Migrated from: AccountMasterData.cs
/// Maps to table: MM_AccountMasterData
/// </summary>
public class Account : AuditableEntity
{
    /// <summary>
    /// Account name.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Account number (was "Conto" in old model).
    /// </summary>
    public string? AccountNumber { get; set; }

    /// <summary>
    /// Account description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// International Bank Account Number.
    /// </summary>
    public string? Iban { get; set; }

    /// <summary>
    /// Bank Identifier Code (SWIFT code).
    /// </summary>
    public string? Bic { get; set; }

    /// <summary>
    /// Type of account (e.g., "Checking", "Savings", "Credit Card").
    /// </summary>
    public string? AccountType { get; set; }

    // Foreign keys
    /// <summary>
    /// Foreign key to Currency (Ancillary service).
    /// </summary>
    public int? CurrencyId { get; set; }

    /// <summary>
    /// Foreign key to Bank.
    /// Maps to column: BankMasterDataId
    /// </summary>
    public int? BankId { get; set; }

    // Navigation properties
    /// <summary>
    /// The bank that owns this account.
    /// </summary>
    public Bank? Bank { get; set; }

    /// <summary>
    /// Collection of transactions for this account.
    /// </summary>
    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();

    /// <summary>
    /// Collection of balance snapshots for this account.
    /// </summary>
    public ICollection<Balance> Balances { get; set; } = new List<Balance>();

    /// <summary>
    /// Collection of cards associated with this account.
    /// </summary>
    public ICollection<Card> Cards { get; set; } = new List<Card>();
}
