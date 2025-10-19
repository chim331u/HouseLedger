using HouseLedger.Services.Ancillary.Application.Contracts.Countries;

namespace HouseLedger.Services.Ancillary.Application.Interfaces;

/// <summary>
/// Query service for Country entities.
/// </summary>
public interface ICountryQueryService
{
    /// <summary>
    /// Get country by ID.
    /// </summary>
    Task<CountryDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all active countries.
    /// </summary>
    Task<IEnumerable<CountryDto>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get country by ISO 3166-1 alpha-3 code (e.g., "USA", "GBR").
    /// </summary>
    Task<CountryDto?> GetByCodeAsync(string countryCodeAlf3, CancellationToken cancellationToken = default);
}
