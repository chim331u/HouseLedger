namespace HouseLedger.Services.HouseThings.Application.Contracts.HouseThings;

public record CreateHouseThingRequest
{
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string? ItemType { get; init; }
    public string? Model { get; init; }
    public double Cost { get; init; }
    public int HistoryId { get; init; }
    public DateTime PurchaseDate { get; init; }
    public int? RoomId { get; init; }
    public string? Note { get; init; }
}
