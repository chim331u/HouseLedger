namespace HouseLedger.Core.Domain.Common;

/// <summary>
/// Base class for entities that require auditing.
/// Tracks creation, modification dates and active status.
/// Migrated from old BaseEntity.cs
/// </summary>
public abstract class AuditableEntity : BaseEntity
{
    /// <summary>
    /// Date and time when the entity was created.
    /// </summary>
    public DateTime CreatedDate { get; set; }

    /// <summary>
    /// Date and time when the entity was last updated.
    /// </summary>
    public DateTime LastUpdatedDate { get; set; }

    /// <summary>
    /// Indicates whether the entity is active or soft-deleted.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Optional notes or comments about the entity.
    /// </summary>
    public string? Note { get; set; }
}
