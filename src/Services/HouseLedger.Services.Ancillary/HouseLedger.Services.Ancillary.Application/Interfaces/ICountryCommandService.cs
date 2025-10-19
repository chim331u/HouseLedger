using HouseLedger.Services.Ancillary.Application.Contracts.Countries;

namespace HouseLedger.Services.Ancillary.Application.Interfaces;

/// <summary>
/// Command service for Country entity CRUD operations.
/// </summary>
public interface ICountryCommandService
{
    /// <summary>
    /// Creates a new country.
    /// </summary>
    Task<CountryDto> CreateAsync(CreateCountryRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing country.
    /// </summary>
    Task<CountryDto?> UpdateAsync(int id, UpdateCountryRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Soft deletes a country by setting IsActive = false.
    /// </summary>
    Task<bool> SoftDeleteAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Hard deletes a country from the database.
    /// </summary>
    Task<bool> HardDeleteAsync(int id, CancellationToken cancellationToken = default);
}
