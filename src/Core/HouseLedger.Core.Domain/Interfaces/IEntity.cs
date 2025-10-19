namespace HouseLedger.Core.Domain.Interfaces;

/// <summary>
/// Marker interface for all entities.
/// </summary>
public interface IEntity
{
    /// <summary>
    /// Unique identifier for the entity.
    /// </summary>
    int Id { get; set; }
}
