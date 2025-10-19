using HouseLedger.Core.Domain.Common;

namespace HouseLedger.Services.Ancillary.Domain.Entities;

/// <summary>
/// Represents a supplier (utility company, service provider, etc.).
/// Used by Bills service to track bill suppliers.
/// Migrated from: MM_RESTAPI/Data/AncillaryData/Supplier.cs
/// Maps to table: Suppliers
/// </summary>
public class Supplier : AuditableEntity
{
    /// <summary>
    /// Supplier name (e.g., "British Gas", "Thames Water").
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Unit of measure for billing (e.g., "kWh", "m³", "GB").
    /// </summary>
    public string? UnitMeasure { get; set; }

    /// <summary>
    /// Supplier description or additional details.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Supplier type (e.g., "Utility", "Service", "Subscription").
    /// </summary>
    public string? Type { get; set; }

    /// <summary>
    /// Contract number or reference.
    /// </summary>
    public string? Contract { get; set; }
}
