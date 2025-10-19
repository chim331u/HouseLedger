namespace HouseLedger.Services.Ancillary.Application.Contracts.Suppliers;

/// <summary>
/// Request DTO for updating an existing supplier.
/// </summary>
public class UpdateSupplierRequest
{
    public string Name { get; set; } = string.Empty;
    public string? UnitMeasure { get; set; }
    public string? Description { get; set; }
    public string? Type { get; set; }
    public string? Contract { get; set; }
    public string? Note { get; set; }
}
