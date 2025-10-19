using HouseLedger.Services.Ancillary.Application.Contracts.Suppliers;

namespace HouseLedger.Services.Ancillary.Application.Interfaces;

/// <summary>
/// Query service for Supplier entities.
/// </summary>
public interface ISupplierQueryService
{
    /// <summary>
    /// Gets a supplier by ID.
    /// </summary>
    Task<SupplierDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all active suppliers.
    /// </summary>
    Task<IEnumerable<SupplierDto>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets suppliers by type.
    /// </summary>
    Task<IEnumerable<SupplierDto>> GetByTypeAsync(string type, CancellationToken cancellationToken = default);
}
