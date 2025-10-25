namespace HouseLedger.Services.HouseThings.Application.Contracts.HouseThings;

public class HouseThingDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ItemType { get; set; }
    public string? Model { get; set; }
    public double Cost { get; set; }
    public int HistoryId { get; set; }
    public DateTime PurchaseDate { get; set; }
    public int? RoomId { get; set; }
    public string? RoomName { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? LastUpdatedDate { get; set; }
    public bool IsActive { get; set; }
    public string? Note { get; set; }
}
