using HouseLedger.Core.Domain.Common;

namespace HouseLedger.Services.Finance.Domain.Entities;

/// <summary>
/// Represents a payment card (debit/credit card).
/// Migrated from: CardMasterData.cs
/// Maps to table: MM_CardMasterData
/// </summary>
public class Card : AuditableEntity
{
    /// <summary>
    /// Card name or alias.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Card number (last 4 digits for security).
    /// </summary>
    public string? CardNumber { get; set; }

    /// <summary>
    /// Card type (e.g., "Visa", "Mastercard", "Amex").
    /// </summary>
    public string? CardType { get; set; }

    /// <summary>
    /// Card expiration date.
    /// </summary>
    public DateTime? ExpirationDate { get; set; }

    /// <summary>
    /// Cardholder name.
    /// </summary>
    public string? CardholderName { get; set; }

    // Foreign keys
    /// <summary>
    /// Foreign key to Account (the account this card is linked to).
    /// </summary>
    public int? AccountId { get; set; }

    // Navigation properties
    /// <summary>
    /// The account this card is linked to.
    /// </summary>
    public Account? Account { get; set; }
}
