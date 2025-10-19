namespace HouseLedger.Services.Ancillary.Application.Contracts.Currencies;

/// <summary>
/// Request DTO for creating a new currency.
/// </summary>
public class CreateCurrencyRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string CurrencyCodeAlf3 { get; set; } = string.Empty;
    public string? CurrencyCodeNum3 { get; set; }
    public string? Note { get; set; }
}
