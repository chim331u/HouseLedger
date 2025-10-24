namespace HouseLedger.Services.Ancillary.Application.Contracts.ServiceUsers;

/// <summary>
/// DTO for ServiceUser entity.
/// </summary>
public record ServiceUserDto
{
    public int Id { get; init; }
    public string? Name { get; init; }
    public string? Surname { get; init; }
    public DateTime CreatedDate { get; init; }
    public DateTime LastUpdatedDate { get; init; }
    public bool IsActive { get; init; }
    public string? Note { get; init; }
}
