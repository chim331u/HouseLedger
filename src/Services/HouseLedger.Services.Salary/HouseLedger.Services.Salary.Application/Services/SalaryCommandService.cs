using AutoMapper;
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

    public SalaryCommandService(
        SalaryDbContext context,
        IMapper mapper,
        ILogger<SalaryCommandService> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<SalaryDto> CreateAsync(CreateSalaryRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating new salary for user {UserId}", request.UserId);

        var salary = _mapper.Map<Domain.Entities.Salary>(request);

        // Set calculated fields
        salary.ExchangeRate = 1.0m; // TODO: Get from CurrencyConversionRate service
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

        // Recalculate EUR value
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
}
