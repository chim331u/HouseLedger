using HouseLedger.Core.Domain.Common;

namespace HouseLedger.Services.Ancillary.Domain.Entities;

/// <summary>
/// Represents a country.
/// Reference data used by Bank and other entities.
/// Migrated from: MM_RESTAPI/Data/AncillaryData/Country.cs
/// </summary>
public class Country : AuditableEntity
{
    /// <summary>
    /// Country name (e.g., "United States", "United Kingdom").
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Country description or additional details.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// ISO 3166-1 alpha-3 country code (e.g., "USA", "GBR").
    /// 3-character code.
    /// </summary>
    public string CountryCodeAlf3 { get; set; } = string.Empty;

    /// <summary>
    /// ISO 3166-1 numeric country code (e.g., "840" for USA, "826" for GBR).
    /// 3-digit code.
    /// </summary>
    public string? CountryCodeNum3 { get; set; }
}
