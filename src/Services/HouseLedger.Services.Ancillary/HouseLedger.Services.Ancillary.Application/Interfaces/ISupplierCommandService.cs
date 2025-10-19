using HouseLedger.Services.Ancillary.Application.Contracts.Suppliers;

namespace HouseLedger.Services.Ancillary.Application.Interfaces;

/// <summary>
/// Command service for Supplier entity CRUD operations.
/// </summary>
public interface ISupplierCommandService
{
    /// <summary>
    /// Creates a new supplier.
    /// </summary>
    Task<SupplierDto> CreateAsync(CreateSupplierRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing supplier.
    /// </summary>
    Task<SupplierDto?> UpdateAsync(int id, UpdateSupplierRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Soft deletes a supplier by setting IsActive = false.
    /// </summary>
    Task<bool> SoftDeleteAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Hard deletes a supplier from the database.
    /// </summary>
    Task<bool> HardDeleteAsync(int id, CancellationToken cancellationToken = default);
}
