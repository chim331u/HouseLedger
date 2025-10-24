using AutoMapper;
using HouseLedger.Services.Ancillary.Application.Interfaces;
using HouseLedger.Services.Salary.Application.Contracts.Salaries;
using HouseLedger.Services.Salary.Application.Interfaces;
using HouseLedger.Services.Salary.Domain.Entities;
using HouseLedger.Services.Salary.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HouseLedger.Services.Salary.Application.Services;

/// <summary>
/// Command service implementation for Salary entity CRUD operations.
/// </summary>
public class SalaryCommandService : ISalaryCommandService
{
    private readonly SalaryDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<SalaryCommandService> _logger;
    private readonly ICurrencyQueryService _currencyQueryService;
    private readonly ICurrencyConversionRateQueryService _conversionRateQueryService;

    public SalaryCommandService(
        SalaryDbContext context,
        IMapper mapper,
        ILogger<SalaryCommandService> logger,
        ICurrencyQueryService currencyQueryService,
        ICurrencyConversionRateQueryService conversionRateQueryService)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
        _currencyQueryService = currencyQueryService;
        _conversionRateQueryService = conversionRateQueryService;
    }

    public async Task<SalaryDto> CreateAsync(CreateSalaryRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating new salary for user {UserId}", request.UserId);

        var salary = _mapper.Map<Domain.Entities.Salary>(request);

        // Retrieve exchange rate from CurrencyConversionRate service
        salary.ExchangeRate = await GetExchangeRateAsync(salary.CurrencyId, salary.SalaryDate, cancellationToken);
        salary.SalaryValueEur = salary.SalaryValue / salary.ExchangeRate;

        // Explicitly initialize audit fields to ensure they're not null
        // DbContext.UpdateAuditFields() will override these with correct values
        var now = DateTime.UtcNow;
        salary.CreatedDate = now;
        salary.LastUpdatedDate = now;
        salary.IsActive = true;

        _logger.LogDebug("Salary entity created: IsActive={IsActive}, CreatedDate={CreatedDate}",
            salary.IsActive, salary.CreatedDate);

        _context.Salaries.Add(salary);

        _logger.LogDebug("Salary entity added to context, calling SaveChangesAsync");
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Salary created successfully with ID: {Id}", salary.Id);

        return _mapper.Map<SalaryDto>(salary);
    }

    public async Task<SalaryDto?> UpdateAsync(int id, UpdateSalaryRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating salary with ID: {Id}", id);

        var salary = await _context.Salaries
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

        if (salary == null)
        {
            _logger.LogWarning("Salary with ID {Id} not found", id);
            return null;
        }

        _logger.LogDebug("Found salary entity, updating properties");

        // Map request properties to entity
        _mapper.Map(request, salary);

        // Recalculate exchange rate and EUR value
        salary.ExchangeRate = await GetExchangeRateAsync(salary.CurrencyId, salary.SalaryDate, cancellationToken);
        salary.SalaryValueEur = salary.SalaryValue / salary.ExchangeRate;

        // LastUpdatedDate is set automatically by DbContext.UpdateAuditFields()
        _logger.LogDebug("Salary entity state: {State}", _context.Entry(salary).State);

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Salary with ID {Id} updated successfully", id);

        return _mapper.Map<SalaryDto>(salary);
    }

    public async Task<bool> SoftDeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Soft deleting salary with ID: {Id}", id);

        var salary = await _context.Salaries
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

        if (salary == null)
        {
            _logger.LogWarning("Salary with ID {Id} not found", id);
            return false;
        }

        _logger.LogDebug("Setting IsActive to false for salary ID: {Id}", id);
        salary.IsActive = false;

        // LastUpdatedDate is set automatically by DbContext.UpdateAuditFields()
        _logger.LogDebug("Salary entity state: {State}", _context.Entry(salary).State);

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Salary with ID {Id} soft deleted successfully", id);

        return true;
    }

    public async Task<bool> HardDeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        _logger.LogWarning("Hard deleting salary with ID: {Id}", id);

        var salary = await _context.Salaries
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

        if (salary == null)
        {
            _logger.LogWarning("Salary with ID {Id} not found", id);
            return false;
        }

        _context.Salaries.Remove(salary);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogWarning("Salary with ID {Id} hard deleted permanently", id);

        return true;
    }

    /// <summary>
    /// Retrieves the latest exchange rate for a given currency to EUR.
    /// If currency is EUR or not specified, returns 1.0.
    /// If currency is not found or no conversion rate exists, returns 1.0 with a warning.
    /// </summary>
    private async Task<decimal> GetExchangeRateAsync(int? currencyId, DateTime salaryDate, CancellationToken cancellationToken = default)
    {
        // If no currency specified, assume EUR (rate = 1.0)
        if (currencyId == null)
        {
            _logger.LogDebug("No currency specified, using default exchange rate 1.0 (EUR)");
            return 1.0m;
        }

        // Get currency details
        var currency = await _currencyQueryService.GetByIdAsync(currencyId.Value, cancellationToken);

        if (currency == null)
        {
            _logger.LogWarning("Currency with ID {CurrencyId} not found, using default exchange rate 1.0", currencyId);
            return 1.0m;
        }

        // If currency is EUR, no conversion needed
        if (currency.CurrencyCodeAlf3?.Equals("EUR", StringComparison.OrdinalIgnoreCase) == true)
        {
            _logger.LogDebug("Currency is EUR, using exchange rate 1.0");
            return 1.0m;
        }

        // Try to get conversion rate for the specific salary date first
        var conversionRate = await _conversionRateQueryService.GetByCurrencyAndDateAsync(
            currency.CurrencyCodeAlf3!,
            salaryDate,
            cancellationToken);

        if (conversionRate != null)
        {
            _logger.LogInformation("Found exchange rate {Rate} for currency {Currency} on date {Date}",
                conversionRate.RateValue, currency.CurrencyCodeAlf3, salaryDate);
            return conversionRate.RateValue;
        }

        // If no rate for specific date, get the latest available rate
        var rates = await _conversionRateQueryService.GetByCurrencyCodeAsync(currency.CurrencyCodeAlf3!, cancellationToken);
        var latestRate = rates.OrderByDescending(r => r.ReferringDate).FirstOrDefault();

        if (latestRate != null)
        {
            _logger.LogWarning("No exchange rate found for currency {Currency} on date {Date}, using latest rate {Rate} from {RateDate}",
                currency.CurrencyCodeAlf3, salaryDate, latestRate.RateValue, latestRate.ReferringDate);
            return latestRate.RateValue;
        }

        // No rate found at all, default to 1.0 with warning
        _logger.LogWarning("No exchange rate found for currency {Currency}, using default rate 1.0", currency.CurrencyCodeAlf3);
        return 1.0m;
    }
}
