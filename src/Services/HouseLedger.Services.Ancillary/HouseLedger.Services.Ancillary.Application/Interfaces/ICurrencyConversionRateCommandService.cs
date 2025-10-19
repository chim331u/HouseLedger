using HouseLedger.Services.Ancillary.Application.Contracts.CurrencyConversionRates;

namespace HouseLedger.Services.Ancillary.Application.Interfaces;

/// <summary>
/// Command service for CurrencyConversionRate entity CRUD operations.
/// </summary>
public interface ICurrencyConversionRateCommandService
{
    /// <summary>
    /// Creates a new currency conversion rate.
    /// </summary>
    Task<CurrencyConversionRateDto> CreateAsync(CreateCurrencyConversionRateRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing currency conversion rate.
    /// </summary>
    Task<CurrencyConversionRateDto?> UpdateAsync(int id, UpdateCurrencyConversionRateRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Soft deletes a currency conversion rate by setting IsActive = false.
    /// </summary>
    Task<bool> SoftDeleteAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Hard deletes a currency conversion rate from the database.
    /// </summary>
    Task<bool> HardDeleteAsync(int id, CancellationToken cancellationToken = default);
}
