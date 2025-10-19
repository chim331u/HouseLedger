namespace HouseLedger.Services.Ancillary.Application.Contracts.Countries;

/// <summary>
/// Request DTO for updating an existing country.
/// </summary>
public class UpdateCountryRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string CountryCodeAlf3 { get; set; } = string.Empty;
    public string? CountryCodeNum3 { get; set; }
    public string? Note { get; set; }
}
