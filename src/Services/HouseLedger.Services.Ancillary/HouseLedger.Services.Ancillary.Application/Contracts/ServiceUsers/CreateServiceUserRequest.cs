namespace HouseLedger.Services.Ancillary.Application.Contracts.ServiceUsers;

/// <summary>
/// Request to create a new ServiceUser.
/// </summary>
public record CreateServiceUserRequest
{
    public string? Name { get; init; }
    public string? Surname { get; init; }
    public string? Note { get; init; }
}
