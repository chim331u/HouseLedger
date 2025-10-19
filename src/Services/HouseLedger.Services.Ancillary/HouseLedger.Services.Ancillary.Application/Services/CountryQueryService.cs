using AutoMapper;
using HouseLedger.Services.Ancillary.Application.Contracts.Countries;
using HouseLedger.Services.Ancillary.Application.Interfaces;
using HouseLedger.Services.Ancillary.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HouseLedger.Services.Ancillary.Application.Services;

/// <summary>
/// Query service for Country entities.
/// </summary>
public class CountryQueryService : ICountryQueryService
{
    private readonly AncillaryDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<CountryQueryService> _logger;

    public CountryQueryService(
        AncillaryDbContext context,
        IMapper mapper,
        ILogger<CountryQueryService> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<CountryDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting country by ID: {CountryId}", id);

        var country = await _context.Countries
            .FirstOrDefaultAsync(c => c.Id == id && c.IsActive, cancellationToken);

        if (country == null)
        {
            _logger.LogWarning("Country not found: {CountryId}", id);
            return null;
        }

        _logger.LogInformation("Country found: {CountryId} - {CountryName}", id, country.Name);
        return _mapper.Map<CountryDto>(country);
    }

    public async Task<IEnumerable<CountryDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting all active countries");

        var countries = await _context.Countries
            .Where(c => c.IsActive)
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);

        _logger.LogInformation("Retrieved {Count} active countries", countries.Count);
        return _mapper.Map<IEnumerable<CountryDto>>(countries);
    }

    public async Task<CountryDto?> GetByCodeAsync(string countryCodeAlf3, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting country by code: {CountryCode}", countryCodeAlf3);

        var country = await _context.Countries
            .FirstOrDefaultAsync(c => c.CountryCodeAlf3 == countryCodeAlf3 && c.IsActive, cancellationToken);

        if (country == null)
        {
            _logger.LogWarning("Country not found with code: {CountryCode}", countryCodeAlf3);
            return null;
        }

        _logger.LogInformation("Country found: {CountryCode} - {CountryName}", countryCodeAlf3, country.Name);
        return _mapper.Map<CountryDto>(country);
    }
}
