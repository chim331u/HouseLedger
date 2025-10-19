using HouseLedger.Core.Domain.Common;

namespace HouseLedger.Services.Finance.Domain.Entities;

/// <summary>
/// Represents a bank institution.
/// Migrated from: BankMasterData.cs
/// Maps to table: MM_BankMasterData
/// </summary>
public class Bank : AuditableEntity
{
    /// <summary>
    /// Bank name (required).
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Bank description (max 50 characters).
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Bank website URL.
    /// </summary>
    public string? WebUrl { get; set; }

    /// <summary>
    /// Physical address of the bank.
    /// </summary>
    public string? Address { get; set; }

    /// <summary>
    /// City where the bank is located.
    /// </summary>
    public string? City { get; set; }

    /// <summary>
    /// Bank contact phone number.
    /// </summary>
    public string? Phone { get; set; }

    /// <summary>
    /// Bank contact email.
    /// </summary>
    public string? Mail { get; set; }

    /// <summary>
    /// Name of the bank contact person.
    /// </summary>
    public string? ReferenceName { get; set; }

    /// <summary>
    /// Foreign key to Country (will be mapped in Infrastructure).
    /// Note: Country is in Ancillary service, so we store FK only.
    /// </summary>
    public int? CountryId { get; set; }

    // Navigation properties
    /// <summary>
    /// Collection of accounts belonging to this bank.
    /// </summary>
    public ICollection<Account> Accounts { get; set; } = new List<Account>();
}
