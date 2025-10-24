namespace HouseLedger.Services.Ancillary.Application.Contracts.ServiceUsers;

/// <summary>
/// Request to update an existing ServiceUser.
/// </summary>
public record UpdateServiceUserRequest
{
    public int Id { get; init; }
    public string? Name { get; init; }
    public string? Surname { get; init; }
    public string? Note { get; init; }
}
