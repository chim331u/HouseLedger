namespace HouseLedger.Services.Ancillary.Application.Contracts.Currencies;

/// <summary>
/// Data transfer object for Currency entity.
/// </summary>
public class CurrencyDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string CurrencyCodeAlf3 { get; set; } = string.Empty;
    public string? CurrencyCodeNum3 { get; set; }

    // Audit fields
    public DateTime CreatedDate { get; set; }
    public DateTime LastUpdatedDate { get; set; }
    public bool IsActive { get; set; }
    public string? Note { get; set; }
}
