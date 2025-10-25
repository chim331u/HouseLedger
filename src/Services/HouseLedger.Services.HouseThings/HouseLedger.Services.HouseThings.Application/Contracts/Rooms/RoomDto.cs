namespace HouseLedger.Services.HouseThings.Application.Contracts.Rooms;

public class RoomDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Color { get; set; }
    public string? Icon { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? LastUpdatedDate { get; set; }
    public bool IsActive { get; set; }
    public string? Note { get; set; }
}
