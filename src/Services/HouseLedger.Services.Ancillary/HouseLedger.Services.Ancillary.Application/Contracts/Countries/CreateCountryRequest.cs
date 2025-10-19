namespace HouseLedger.Services.Ancillary.Application.Contracts.Countries;

/// <summary>
/// Request DTO for creating a new country.
/// </summary>
public class CreateCountryRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string CountryCodeAlf3 { get; set; } = string.Empty;
    public string? CountryCodeNum3 { get; set; }
    public string? Note { get; set; }
}
