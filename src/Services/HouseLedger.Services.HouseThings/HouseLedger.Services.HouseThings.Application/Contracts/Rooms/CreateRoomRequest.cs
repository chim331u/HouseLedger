namespace HouseLedger.Services.HouseThings.Application.Contracts.Rooms;

public record CreateRoomRequest
{
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string? Color { get; init; }
    public string? Icon { get; init; }
    public string? Note { get; init; }
}
