namespace HouseLedger.Services.Ancillary.Application.Contracts.Suppliers;

/// <summary>
/// Data transfer object for Supplier entity.
/// </summary>
public class SupplierDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? UnitMeasure { get; set; }
    public string? Description { get; set; }
    public string? Type { get; set; }
    public string? Contract { get; set; }

    // Audit fields
    public DateTime CreatedDate { get; set; }
    public DateTime LastUpdatedDate { get; set; }
    public bool IsActive { get; set; }
    public string? Note { get; set; }
}
