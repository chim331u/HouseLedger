namespace HouseLedger.Services.Finance.Application.Contracts.Banks;

/// <summary>
/// Request to update an existing bank.
/// </summary>
public record UpdateBankRequest
{
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string? WebUrl { get; init; }
    public string? Address { get; init; }
    public string? City { get; init; }
    public string? Phone { get; init; }
    public string? Mail { get; init; }
    public string? ReferenceName { get; init; }
    public int? CountryId { get; init; }
    public string? Note { get; init; }
}
