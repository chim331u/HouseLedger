using HouseLedger.Core.Domain.Common;

namespace HouseLedger.Services.Ancillary.Domain.Entities;

/// <summary>
/// Represents a currency conversion rate for a specific date.
/// Used for converting between currencies based on historical rates.
/// Migrated from: MM_RESTAPI/Data/AncillaryData/CurrencyConversionRate.cs
/// Maps to table: AD_CurrencyConversionRate
/// </summary>
public class CurrencyConversionRate : AuditableEntity
{
    /// <summary>
    /// The conversion rate value (e.g., 1.12 for EUR to USD).
    /// </summary>
    public decimal RateValue { get; set; }

    /// <summary>
    /// ISO 4217 alphabetic currency code (e.g., "USD", "EUR").
    /// The base currency for this conversion rate.
    /// </summary>
    public string CurrencyCodeAlf3 { get; set; } = string.Empty;

    /// <summary>
    /// The date this conversion rate is valid for.
    /// Only the date portion is used (time is ignored).
    /// </summary>
    public DateTime ReferringDate { get; set; }

    /// <summary>
    /// Unique key for deduplication.
    /// Format: concat(CurrencyCodeAlf3 + RateValue + ReferringDate)
    /// Example: "USD1.12202501-19"
    /// </summary>
    public string? UniqueKey { get; set; }
}
