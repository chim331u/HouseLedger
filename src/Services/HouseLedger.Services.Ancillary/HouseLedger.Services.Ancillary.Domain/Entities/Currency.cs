using HouseLedger.Core.Domain.Common;

namespace HouseLedger.Services.Ancillary.Domain.Entities;

/// <summary>
/// Represents a currency (e.g., USD, EUR, GBP).
/// Reference data used by Bank and other entities.
/// Migrated from: MM_RESTAPI/Data/AncillaryData/Currency.cs
/// </summary>
public class Currency : AuditableEntity
{
    /// <summary>
    /// Currency name (e.g., "US Dollar", "Euro").
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Currency description or additional details.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// ISO 4217 alphabetic currency code (e.g., "USD", "EUR").
    /// 3-character code.
    /// </summary>
    public string CurrencyCodeAlf3 { get; set; } = string.Empty;

    /// <summary>
    /// ISO 4217 numeric currency code (e.g., "840" for USD, "978" for EUR).
    /// 3-digit code.
    /// </summary>
    public string? CurrencyCodeNum3 { get; set; }
}
