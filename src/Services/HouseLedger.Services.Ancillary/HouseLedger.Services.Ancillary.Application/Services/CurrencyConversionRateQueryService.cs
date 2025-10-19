using AutoMapper;
using HouseLedger.Services.Ancillary.Application.Contracts.CurrencyConversionRates;
using HouseLedger.Services.Ancillary.Application.Interfaces;
using HouseLedger.Services.Ancillary.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HouseLedger.Services.Ancillary.Application.Services;

/// <summary>
/// Query service implementation for CurrencyConversionRate entities.
/// </summary>
public class CurrencyConversionRateQueryService : ICurrencyConversionRateQueryService
{
    private readonly AncillaryDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<CurrencyConversionRateQueryService> _logger;

    public CurrencyConversionRateQueryService(
        AncillaryDbContext context,
        IMapper mapper,
        ILogger<CurrencyConversionRateQueryService> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<CurrencyConversionRateDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting currency conversion rate with ID: {Id}", id);

        var rate = await _context.CurrencyConversionRates
            .Where(c => c.Id == id && c.IsActive)
            .FirstOrDefaultAsync(cancellationToken);

        if (rate == null)
        {
            _logger.LogWarning("Currency conversion rate with ID {Id} not found", id);
            return null;
        }

        return _mapper.Map<CurrencyConversionRateDto>(rate);
    }

    public async Task<IEnumerable<CurrencyConversionRateDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting all active currency conversion rates");

        var rates = await _context.CurrencyConversionRates
            .Where(c => c.IsActive)
            .OrderByDescending(c => c.ReferringDate)
            .ThenBy(c => c.CurrencyCodeAlf3)
            .ToListAsync(cancellationToken);

        _logger.LogInformation("Retrieved {Count} currency conversion rates", rates.Count);

        return _mapper.Map<IEnumerable<CurrencyConversionRateDto>>(rates);
    }

    public async Task<IEnumerable<CurrencyConversionRateDto>> GetByCurrencyCodeAsync(string currencyCode, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting currency conversion rates for currency code: {CurrencyCode}", currencyCode);

        var rates = await _context.CurrencyConversionRates
            .Where(c => c.CurrencyCodeAlf3 == currencyCode && c.IsActive)
            .OrderByDescending(c => c.ReferringDate)
            .ToListAsync(cancellationToken);

        _logger.LogInformation("Retrieved {Count} currency conversion rates for {CurrencyCode}", rates.Count, currencyCode);

        return _mapper.Map<IEnumerable<CurrencyConversionRateDto>>(rates);
    }

    public async Task<CurrencyConversionRateDto?> GetByCurrencyAndDateAsync(string currencyCode, DateTime date, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting currency conversion rate for {CurrencyCode} on {Date}", currencyCode, date);

        var rate = await _context.CurrencyConversionRates
            .Where(c => c.CurrencyCodeAlf3 == currencyCode && c.ReferringDate.Date == date.Date && c.IsActive)
            .FirstOrDefaultAsync(cancellationToken);

        if (rate == null)
        {
            _logger.LogWarning("Currency conversion rate for {CurrencyCode} on {Date} not found", currencyCode, date);
            return null;
        }

        return _mapper.Map<CurrencyConversionRateDto>(rate);
    }
}
