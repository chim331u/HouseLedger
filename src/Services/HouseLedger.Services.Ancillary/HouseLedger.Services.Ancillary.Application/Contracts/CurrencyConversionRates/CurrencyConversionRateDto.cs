namespace HouseLedger.Services.Ancillary.Application.Contracts.CurrencyConversionRates;

/// <summary>
/// Data transfer object for CurrencyConversionRate entity.
/// </summary>
public class CurrencyConversionRateDto
{
    public int Id { get; set; }
    public decimal RateValue { get; set; }
    public string CurrencyCodeAlf3 { get; set; } = string.Empty;
    public DateTime ReferringDate { get; set; }
    public string? UniqueKey { get; set; }

    // Audit fields
    public DateTime CreatedDate { get; set; }
    public DateTime LastUpdatedDate { get; set; }
    public bool IsActive { get; set; }
    public string? Note { get; set; }
}
