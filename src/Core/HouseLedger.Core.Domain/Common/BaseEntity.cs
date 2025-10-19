namespace HouseLedger.Core.Domain.Common;

/// <summary>
/// Base class for all entities in the system.
/// Provides common properties for entity identification.
/// </summary>
public abstract class BaseEntity
{
    /// <summary>
    /// Unique identifier for the entity.
    /// </summary>
    public int Id { get; set; }
}
