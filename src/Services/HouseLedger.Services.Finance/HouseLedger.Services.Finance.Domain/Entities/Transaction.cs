using HouseLedger.Core.Domain.Common;
using HouseLedger.Services.Finance.Domain.ValueObjects;

namespace HouseLedger.Services.Finance.Domain.Entities;

/// <summary>
/// Represents a financial transaction.
/// Migrated from: Transaction.cs
/// Maps to table: TX_Transaction
/// </summary>
public class Transaction : AuditableEntity
{
    /// <summary>
    /// Transaction date (was TxnDate in old model).
    /// Maps to column: TxnDate
    /// </summary>
    public DateTime TransactionDate { get; set; }

    /// <summary>
    /// Transaction amount (was TxnAmount in old model).
    /// Maps to column: TxnAmount
    /// Kept as double per your requirement.
    /// </summary>
    public double Amount { get; set; }

    /// <summary>
    /// Transaction description.
    /// Maps to column: Description
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Unique key for deduplication (concat currencyCodeAlf3+rateValue+referringDate).
    /// Maps to column: UniqueKey
    /// </summary>
    public string? UniqueKey { get; set; }

    // Backing fields for database columns (EF Core will use these)
    private string? _area;
    private bool _isCatConfirmed;

    /// <summary>
    /// Category name - maps to Area column (for EF Core).
    /// </summary>
    public string? Area
    {
        get => _area;
        set
        {
            _area = value;
            UpdateCategoryFromFields();
        }
    }

    /// <summary>
    /// Category confirmation status - maps to IsCatConfirmed column (for EF Core).
    /// </summary>
    public bool IsCatConfirmed
    {
        get => _isCatConfirmed;
        set
        {
            _isCatConfirmed = value;
            UpdateCategoryFromFields();
        }
    }

    /// <summary>
    /// Transaction category with confirmation status (Value Object).
    /// This is computed from Area + IsCatConfirmed fields.
    /// </summary>
    public TransactionCategory? Category
    {
        get => string.IsNullOrWhiteSpace(_area) ? null : new TransactionCategory(_area, _isCatConfirmed);
        set
        {
            if (value == null)
            {
                _area = null;
                _isCatConfirmed = false;
            }
            else
            {
                _area = value.Name;
                _isCatConfirmed = value.IsConfirmed;
            }
        }
    }

    private void UpdateCategoryFromFields()
    {
        // This ensures Category stays in sync when Area or IsCatConfirmed change
        // (though typically you should use Category property directly)
    }

    // Foreign keys
    /// <summary>
    /// Foreign key to Account.
    /// Maps to: Account navigation property
    /// </summary>
    public int AccountId { get; set; }

    // Navigation properties
    /// <summary>
    /// The account this transaction belongs to.
    /// </summary>
    public Account Account { get; set; } = null!;
}
