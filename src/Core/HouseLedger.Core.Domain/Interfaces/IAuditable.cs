namespace HouseLedger.Core.Domain.Interfaces;

/// <summary>
/// Interface for entities that support auditing.
/// </summary>
public interface IAuditable
{
    DateTime CreatedDate { get; set; }
    DateTime LastUpdatedDate { get; set; }
    bool IsActive { get; set; }
    string? Note { get; set; }
}
