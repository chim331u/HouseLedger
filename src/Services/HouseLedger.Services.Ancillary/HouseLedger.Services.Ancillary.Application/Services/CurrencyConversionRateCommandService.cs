using AutoMapper;
using HouseLedger.Services.Ancillary.Application.Contracts.CurrencyConversionRates;
using HouseLedger.Services.Ancillary.Application.Interfaces;
using HouseLedger.Services.Ancillary.Domain.Entities;
using HouseLedger.Services.Ancillary.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HouseLedger.Services.Ancillary.Application.Services;

/// <summary>
/// Command service implementation for CurrencyConversionRate entity CRUD operations.
/// </summary>
public class CurrencyConversionRateCommandService : ICurrencyConversionRateCommandService
{
    private readonly AncillaryDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<CurrencyConversionRateCommandService> _logger;

    public CurrencyConversionRateCommandService(
        AncillaryDbContext context,
        IMapper mapper,
        ILogger<CurrencyConversionRateCommandService> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<CurrencyConversionRateDto> CreateAsync(CreateCurrencyConversionRateRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating new currency conversion rate for: {CurrencyCode} on {Date}",
            request.CurrencyCodeAlf3, request.ReferringDate);

        var rate = _mapper.Map<CurrencyConversionRate>(request);
        rate.CreatedDate = DateTime.UtcNow;
        rate.LastUpdatedDate = DateTime.UtcNow;
        rate.IsActive = true;

        _context.CurrencyConversionRates.Add(rate);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Currency conversion rate created successfully with ID: {Id}", rate.Id);

        return _mapper.Map<CurrencyConversionRateDto>(rate);
    }

    public async Task<CurrencyConversionRateDto?> UpdateAsync(int id, UpdateCurrencyConversionRateRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating currency conversion rate with ID: {Id}", id);

        var rate = await _context.CurrencyConversionRates
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

        if (rate == null)
        {
            _logger.LogWarning("Currency conversion rate with ID {Id} not found", id);
            return null;
        }

        _mapper.Map(request, rate);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Currency conversion rate with ID {Id} updated successfully", id);

        return _mapper.Map<CurrencyConversionRateDto>(rate);
    }

    public async Task<bool> SoftDeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Soft deleting currency conversion rate with ID: {Id}", id);

        var rate = await _context.CurrencyConversionRates
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

        if (rate == null)
        {
            _logger.LogWarning("Currency conversion rate with ID {Id} not found", id);
            return false;
        }

        rate.IsActive = false;
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Currency conversion rate with ID {Id} soft deleted successfully", id);

        return true;
    }

    public async Task<bool> HardDeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        _logger.LogWarning("Hard deleting currency conversion rate with ID: {Id}", id);

        var rate = await _context.CurrencyConversionRates
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

        if (rate == null)
        {
            _logger.LogWarning("Currency conversion rate with ID {Id} not found", id);
            return false;
        }

        _context.CurrencyConversionRates.Remove(rate);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogWarning("Currency conversion rate with ID {Id} hard deleted permanently", id);

        return true;
    }
}
