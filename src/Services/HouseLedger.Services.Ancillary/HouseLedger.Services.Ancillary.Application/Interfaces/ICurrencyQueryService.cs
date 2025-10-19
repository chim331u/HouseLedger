using HouseLedger.Services.Ancillary.Application.Contracts.Currencies;

namespace HouseLedger.Services.Ancillary.Application.Interfaces;

/// <summary>
/// Query service for Currency entities.
/// </summary>
public interface ICurrencyQueryService
{
    /// <summary>
    /// Get currency by ID.
    /// </summary>
    Task<CurrencyDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all active currencies.
    /// </summary>
    Task<IEnumerable<CurrencyDto>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get currency by ISO 4217 alphabetic code (e.g., "USD", "EUR").
    /// </summary>
    Task<CurrencyDto?> GetByCodeAsync(string currencyCodeAlf3, CancellationToken cancellationToken = default);
}
