using AutoMapper;
using HouseLedger.Services.Ancillary.Application.Contracts.Countries;
using HouseLedger.Services.Ancillary.Application.Interfaces;
using HouseLedger.Services.Ancillary.Domain.Entities;
using HouseLedger.Services.Ancillary.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HouseLedger.Services.Ancillary.Application.Services;

/// <summary>
/// Command service implementation for Country entity CRUD operations.
/// </summary>
public class CountryCommandService : ICountryCommandService
{
    private readonly AncillaryDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<CountryCommandService> _logger;

    public CountryCommandService(
        AncillaryDbContext context,
        IMapper mapper,
        ILogger<CountryCommandService> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<CountryDto> CreateAsync(CreateCountryRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating new country: {CountryCode}", request.CountryCodeAlf3);

        var country = _mapper.Map<Country>(request);
        _context.Countries.Add(country);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Country created successfully with ID: {Id}", country.Id);

        return _mapper.Map<CountryDto>(country);
    }

    public async Task<CountryDto?> UpdateAsync(int id, UpdateCountryRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating country with ID: {Id}", id);

        var country = await _context.Countries
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

        if (country == null)
        {
            _logger.LogWarning("Country with ID {Id} not found", id);
            return null;
        }

        _mapper.Map(request, country);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Country with ID {Id} updated successfully", id);

        return _mapper.Map<CountryDto>(country);
    }

    public async Task<bool> SoftDeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Soft deleting country with ID: {Id}", id);

        var country = await _context.Countries
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

        if (country == null)
        {
            _logger.LogWarning("Country with ID {Id} not found", id);
            return false;
        }

        country.IsActive = false;
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Country with ID {Id} soft deleted successfully", id);

        return true;
    }

    public async Task<bool> HardDeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        _logger.LogWarning("Hard deleting country with ID: {Id}", id);

        var country = await _context.Countries
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

        if (country == null)
        {
            _logger.LogWarning("Country with ID {Id} not found", id);
            return false;
        }

        _context.Countries.Remove(country);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogWarning("Country with ID {Id} hard deleted permanently", id);

        return true;
    }
}
