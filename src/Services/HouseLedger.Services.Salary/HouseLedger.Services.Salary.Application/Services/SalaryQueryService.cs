using AutoMapper;
using HouseLedger.Services.Salary.Application.Contracts.Salaries;
using HouseLedger.Services.Salary.Application.Interfaces;
using HouseLedger.Services.Salary.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HouseLedger.Services.Salary.Application.Services;

/// <summary>
/// Query service implementation for Salary entity read operations.
/// </summary>
public class SalaryQueryService : ISalaryQueryService
{
    private readonly SalaryDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<SalaryQueryService> _logger;

    public SalaryQueryService(
        SalaryDbContext context,
        IMapper mapper,
        ILogger<SalaryQueryService> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IEnumerable<SalaryDto>> GetAllAsync(bool includeInactive = false, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching all salaries (includeInactive: {IncludeInactive})", includeInactive);

        var query = _context.Salaries.AsQueryable();

        if (!includeInactive)
        {
            query = query.Where(s => s.IsActive);
        }

        var salaries = await query
            .OrderByDescending(s => s.SalaryDate)
            .ToListAsync(cancellationToken);

        _logger.LogInformation("Retrieved {Count} salaries", salaries.Count);

        return _mapper.Map<IEnumerable<SalaryDto>>(salaries);
    }

    public async Task<SalaryDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching salary with ID: {Id}", id);

        var salary = await _context.Salaries
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

        if (salary == null)
        {
            _logger.LogWarning("Salary with ID {Id} not found", id);
            return null;
        }

        return _mapper.Map<SalaryDto>(salary);
    }

    public async Task<IEnumerable<SalaryDto>> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching salaries for user ID: {UserId}", userId);

        var salaries = await _context.Salaries
            .Where(s => s.UserId == userId && s.IsActive)
            .OrderByDescending(s => s.SalaryDate)
            .ToListAsync(cancellationToken);

        _logger.LogInformation("Retrieved {Count} salaries for user {UserId}", salaries.Count, userId);

        return _mapper.Map<IEnumerable<SalaryDto>>(salaries);
    }

    public async Task<IEnumerable<SalaryDto>> GetByYearAsync(string year, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching salaries for year: {Year}", year);

        var salaries = await _context.Salaries
            .Where(s => s.ReferYear == year && s.IsActive)
            .OrderByDescending(s => s.SalaryDate)
            .ToListAsync(cancellationToken);

        _logger.LogInformation("Retrieved {Count} salaries for year {Year}", salaries.Count, year);

        return _mapper.Map<IEnumerable<SalaryDto>>(salaries);
    }
}
