using HouseLedger.Services.Ancillary.Application.Contracts.Currencies;

namespace HouseLedger.Services.Ancillary.Application.Interfaces;

/// <summary>
/// Command service for Currency entity CRUD operations.
/// </summary>
public interface ICurrencyCommandService
{
    /// <summary>
    /// Creates a new currency.
    /// </summary>
    Task<CurrencyDto> CreateAsync(CreateCurrencyRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing currency.
    /// </summary>
    Task<CurrencyDto?> UpdateAsync(int id, UpdateCurrencyRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Soft deletes a currency by setting IsActive = false.
    /// </summary>
    Task<bool> SoftDeleteAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Hard deletes a currency from the database.
    /// </summary>
    Task<bool> HardDeleteAsync(int id, CancellationToken cancellationToken = default);
}
