using AutoMapper;
using HouseLedger.Services.Ancillary.Application.Contracts.Currencies;
using HouseLedger.Services.Ancillary.Application.Interfaces;
using HouseLedger.Services.Ancillary.Domain.Entities;
using HouseLedger.Services.Ancillary.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HouseLedger.Services.Ancillary.Application.Services;

/// <summary>
/// Command service implementation for Currency entity CRUD operations.
/// </summary>
public class CurrencyCommandService : ICurrencyCommandService
{
    private readonly AncillaryDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<CurrencyCommandService> _logger;

    public CurrencyCommandService(
        AncillaryDbContext context,
        IMapper mapper,
        ILogger<CurrencyCommandService> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<CurrencyDto> CreateAsync(CreateCurrencyRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating new currency: {CurrencyCode}", request.CurrencyCodeAlf3);

        var currency = _mapper.Map<Currency>(request);
        _context.Currencies.Add(currency);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Currency created successfully with ID: {Id}", currency.Id);

        return _mapper.Map<CurrencyDto>(currency);
    }

    public async Task<CurrencyDto?> UpdateAsync(int id, UpdateCurrencyRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating currency with ID: {Id}", id);

        var currency = await _context.Currencies
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

        if (currency == null)
        {
            _logger.LogWarning("Currency with ID {Id} not found", id);
            return null;
        }

        _mapper.Map(request, currency);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Currency with ID {Id} updated successfully", id);

        return _mapper.Map<CurrencyDto>(currency);
    }

    public async Task<bool> SoftDeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Soft deleting currency with ID: {Id}", id);

        var currency = await _context.Currencies
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

        if (currency == null)
        {
            _logger.LogWarning("Currency with ID {Id} not found", id);
            return false;
        }

        currency.IsActive = false;
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Currency with ID {Id} soft deleted successfully", id);

        return true;
    }

    public async Task<bool> HardDeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        _logger.LogWarning("Hard deleting currency with ID: {Id}", id);

        var currency = await _context.Currencies
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

        if (currency == null)
        {
            _logger.LogWarning("Currency with ID {Id} not found", id);
            return false;
        }

        _context.Currencies.Remove(currency);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogWarning("Currency with ID {Id} hard deleted permanently", id);

        return true;
    }
}
