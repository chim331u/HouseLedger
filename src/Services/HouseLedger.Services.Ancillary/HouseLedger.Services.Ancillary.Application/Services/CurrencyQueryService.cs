using AutoMapper;
using HouseLedger.Services.Ancillary.Application.Contracts.Currencies;
using HouseLedger.Services.Ancillary.Application.Interfaces;
using HouseLedger.Services.Ancillary.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HouseLedger.Services.Ancillary.Application.Services;

/// <summary>
/// Query service for Currency entities.
/// </summary>
public class CurrencyQueryService : ICurrencyQueryService
{
    private readonly AncillaryDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<CurrencyQueryService> _logger;

    public CurrencyQueryService(
        AncillaryDbContext context,
        IMapper mapper,
        ILogger<CurrencyQueryService> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<CurrencyDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting currency by ID: {CurrencyId}", id);

        var currency = await _context.Currencies
            .FirstOrDefaultAsync(c => c.Id == id && c.IsActive, cancellationToken);

        if (currency == null)
        {
            _logger.LogWarning("Currency not found: {CurrencyId}", id);
            return null;
        }

        _logger.LogInformation("Currency found: {CurrencyId} - {CurrencyName}", id, currency.Name);
        return _mapper.Map<CurrencyDto>(currency);
    }

    public async Task<IEnumerable<CurrencyDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting all active currencies");

        var currencies = await _context.Currencies
            .Where(c => c.IsActive)
            .OrderBy(c => c.CurrencyCodeAlf3)
            .ToListAsync(cancellationToken);

        _logger.LogInformation("Retrieved {Count} active currencies", currencies.Count);
        return _mapper.Map<IEnumerable<CurrencyDto>>(currencies);
    }

    public async Task<CurrencyDto?> GetByCodeAsync(string currencyCodeAlf3, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting currency by code: {CurrencyCode}", currencyCodeAlf3);

        var currency = await _context.Currencies
            .FirstOrDefaultAsync(c => c.CurrencyCodeAlf3 == currencyCodeAlf3 && c.IsActive, cancellationToken);

        if (currency == null)
        {
            _logger.LogWarning("Currency not found with code: {CurrencyCode}", currencyCodeAlf3);
            return null;
        }

        _logger.LogInformation("Currency found: {CurrencyCode} - {CurrencyName}", currencyCodeAlf3, currency.Name);
        return _mapper.Map<CurrencyDto>(currency);
    }
}
