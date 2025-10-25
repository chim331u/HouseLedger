using HouseLedger.Core.Domain.Common;

namespace HouseLedger.Services.HouseThings.Domain.Entities;

/// <summary>
/// Represents a room in the house.
/// Migrated from: HouseThingsRooms.cs
/// Maps to table: MM_HouseThingsRooms
/// </summary>
public class Room : AuditableEntity
{
    /// <summary>
    /// Room name (e.g., "Kitchen", "Living Room", "Bedroom").
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Optional description of the room.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Color associated with the room (for UI purposes).
    /// </summary>
    public string? Color { get; set; }

    /// <summary>
    /// Icon name for the room (for UI purposes).
    /// </summary>
    public string? Icon { get; set; }

    // Navigation properties
    /// <summary>
    /// Collection of house things/items in this room.
    /// </summary>
    public ICollection<HouseThing> HouseThings { get; set; } = new List<HouseThing>();
}
