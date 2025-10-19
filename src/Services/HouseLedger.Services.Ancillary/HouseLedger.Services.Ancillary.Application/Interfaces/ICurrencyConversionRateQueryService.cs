using HouseLedger.Services.Ancillary.Application.Contracts.CurrencyConversionRates;

namespace HouseLedger.Services.Ancillary.Application.Interfaces;

/// <summary>
/// Query service for CurrencyConversionRate entities.
/// </summary>
public interface ICurrencyConversionRateQueryService
{
    /// <summary>
    /// Gets a currency conversion rate by ID.
    /// </summary>
    Task<CurrencyConversionRateDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all active currency conversion rates.
    /// </summary>
    Task<IEnumerable<CurrencyConversionRateDto>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets conversion rates for a specific currency code.
    /// </summary>
    Task<IEnumerable<CurrencyConversionRateDto>> GetByCurrencyCodeAsync(string currencyCode, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the conversion rate for a specific currency on a specific date.
    /// </summary>
    Task<CurrencyConversionRateDto?> GetByCurrencyAndDateAsync(string currencyCode, DateTime date, CancellationToken cancellationToken = default);
}
