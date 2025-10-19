using HouseLedger.Core.Domain.Common;

namespace HouseLedger.Services.Finance.Domain.ValueObjects;

/// <summary>
/// Value Object representing a transaction category with confirmation status.
/// Combines the old "Area" (category name) and "IsCatConfirmed" (confirmation flag) fields.
/// Provides type safety and prevents invalid category states.
/// </summary>
public class TransactionCategory : ValueObject
{
    /// <summary>
    /// Category name (e.g., "Groceries", "Utilities", "Salary").
    /// Maps to column: Area
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// Indicates whether the category assignment has been confirmed by the user.
    /// Maps to column: IsCatConfirmed
    /// </summary>
    public bool IsConfirmed { get; private set; }

    /// <summary>
    /// Private constructor for EF Core.
    /// </summary>
    private TransactionCategory()
    {
        Name = string.Empty;
    }

    /// <summary>
    /// Creates a new transaction category.
    /// </summary>
    /// <param name="name">Category name</param>
    /// <param name="isConfirmed">Whether the category is confirmed</param>
    public TransactionCategory(string name, bool isConfirmed = false)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Category name cannot be empty", nameof(name));

        Name = name.Trim();
        IsConfirmed = isConfirmed;
    }

    /// <summary>
    /// Creates a confirmed category.
    /// </summary>
    public static TransactionCategory CreateConfirmed(string name)
        => new(name, true);

    /// <summary>
    /// Creates an unconfirmed category (e.g., from ML prediction).
    /// </summary>
    public static TransactionCategory CreateUnconfirmed(string name)
        => new(name, false);

    /// <summary>
    /// Returns a new category with confirmed status.
    /// Value objects are immutable, so this creates a new instance.
    /// </summary>
    public TransactionCategory Confirm()
        => new(Name, true);

    /// <summary>
    /// Changes the category name (returns new instance as value objects are immutable).
    /// </summary>
    public TransactionCategory ChangeName(string newName)
        => new(newName, IsConfirmed);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Name;
        yield return IsConfirmed;
    }

    public override string ToString()
        => IsConfirmed ? $"{Name} (Confirmed)" : $"{Name} (Unconfirmed)";

    // Common predefined categories (optional, can be extended)
    public static readonly TransactionCategory Groceries = CreateUnconfirmed("Groceries");
    public static readonly TransactionCategory Utilities = CreateUnconfirmed("Utilities");
    public static readonly TransactionCategory Rent = CreateUnconfirmed("Rent");
    public static readonly TransactionCategory Transportation = CreateUnconfirmed("Transportation");
    public static readonly TransactionCategory Entertainment = CreateUnconfirmed("Entertainment");
    public static readonly TransactionCategory Healthcare = CreateUnconfirmed("Healthcare");
    public static readonly TransactionCategory Education = CreateUnconfirmed("Education");
    public static readonly TransactionCategory Salary = CreateConfirmed("Salary");
    public static readonly TransactionCategory Investment = CreateUnconfirmed("Investment");
    public static readonly TransactionCategory Other = CreateUnconfirmed("Other");
}
