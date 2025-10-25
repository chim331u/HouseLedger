namespace HouseLedger.Services.Finance.Application.Contracts.Banks;

/// <summary>
/// Data transfer object for Bank entity.
/// </summary>
public class BankDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? WebUrl { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Phone { get; set; }
    public string? Mail { get; set; }
    public string? ReferenceName { get; set; }
    public int? CountryId { get; set; }

    // Audit fields
    public DateTime CreatedDate { get; set; }
    public DateTime? LastUpdatedDate { get; set; }
    public bool IsActive { get; set; }
    public string? Note { get; set; }
}
