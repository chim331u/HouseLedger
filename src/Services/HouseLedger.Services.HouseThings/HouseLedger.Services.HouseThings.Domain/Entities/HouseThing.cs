using HouseLedger.Core.Domain.Common;

namespace HouseLedger.Services.HouseThings.Domain.Entities;

/// <summary>
/// Represents a household item/thing.
/// Migrated from: HouseThings.cs
/// Maps to table: MM_HouseThings
/// </summary>
public class HouseThing : AuditableEntity
{
    /// <summary>
    /// Name of the household item.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Description of the item.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Type/category of the item (e.g., "Appliance", "Furniture", "Electronics").
    /// </summary>
    public string? ItemType { get; set; }

    /// <summary>
    /// Model number or name.
    /// </summary>
    public string? Model { get; set; }

    /// <summary>
    /// Purchase cost of the item.
    /// </summary>
    public double Cost { get; set; }

    /// <summary>
    /// History ID for tracking replacements/renewals of the same item.
    /// Items with the same HistoryId represent the same thing replaced over time.
    /// </summary>
    public int HistoryId { get; set; }

    /// <summary>
    /// Date when the item was purchased.
    /// </summary>
    public DateTime PurchaseDate { get; set; }

    // Foreign keys
    /// <summary>
    /// Foreign key to Room.
    /// </summary>
    public int? RoomId { get; set; }

    // Navigation properties
    /// <summary>
    /// The room where this item is located.
    /// </summary>
    public Room? Room { get; set; }
}
