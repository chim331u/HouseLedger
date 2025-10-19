namespace HouseLedger.Services.Ancillary.Application.Contracts.CurrencyConversionRates;

/// <summary>
/// Request DTO for updating an existing currency conversion rate.
/// </summary>
public class UpdateCurrencyConversionRateRequest
{
    public decimal RateValue { get; set; }
    public string CurrencyCodeAlf3 { get; set; } = string.Empty;
    public DateTime ReferringDate { get; set; }
    public string? UniqueKey { get; set; }
    public string? Note { get; set; }
}
