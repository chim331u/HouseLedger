using HouseLedger.Core.Domain.Common;

namespace HouseLedger.Services.Ancillary.Domain.Entities;

/// <summary>
/// Represents a service user (person/employee).
/// Used to track individuals associated with salaries, transactions, and other operations.
/// Migrated from: MM_RESTAPI/Data/AncillaryData/ServiceUser.cs
/// </summary>
public class ServiceUser : AuditableEntity
{
    /// <summary>
    /// User's first name.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// User's surname/last name.
    /// </summary>
    public string? Surname { get; set; }
}
