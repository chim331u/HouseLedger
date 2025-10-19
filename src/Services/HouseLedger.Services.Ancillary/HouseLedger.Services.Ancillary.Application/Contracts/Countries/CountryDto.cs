namespace HouseLedger.Services.Ancillary.Application.Contracts.Countries;

/// <summary>
/// Data transfer object for Country entity.
/// </summary>
public class CountryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string CountryCodeAlf3 { get; set; } = string.Empty;
    public string? CountryCodeNum3 { get; set; }

    // Audit fields
    public DateTime CreatedDate { get; set; }
    public DateTime LastUpdatedDate { get; set; }
    public bool IsActive { get; set; }
    public string? Note { get; set; }
}
