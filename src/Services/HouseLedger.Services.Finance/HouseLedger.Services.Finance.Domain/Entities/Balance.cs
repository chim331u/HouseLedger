using HouseLedger.Core.Domain.Common;

namespace HouseLedger.Services.Finance.Domain.Entities;

/// <summary>
/// Represents a balance snapshot for an account at a specific point in time.
/// Migrated from: Balance.cs
/// Maps to table: MM_Balance
/// </summary>
public class Balance : AuditableEntity
{
    /// <summary>
    /// Balance amount (was BalanceValue in old model).
    /// Maps to column: BalanceValue
    /// Kept as double per your requirement.
    /// </summary>
    public double Amount { get; set; }

    /// <summary>
    /// Date when this balance was recorded (was DateBalance in old model).
    /// Maps to column: DateBalance
    /// </summary>
    public DateTime BalanceDate { get; set; }

    // Foreign keys
    /// <summary>
    /// Foreign key to Account.
    /// Maps to: Account navigation property
    /// </summary>
    public int? AccountId { get; set; }

    // Navigation properties
    /// <summary>
    /// The account this balance belongs to.
    /// </summary>
    public Account? Account { get; set; }
}
