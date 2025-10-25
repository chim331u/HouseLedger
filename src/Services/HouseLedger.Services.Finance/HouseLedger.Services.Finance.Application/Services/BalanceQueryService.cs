using AutoMapper;
using HouseLedger.Services.Finance.Application.Contracts.Balances;
using HouseLedger.Services.Finance.Application.Interfaces;
using HouseLedger.Services.Finance.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HouseLedger.Services.Finance.Application.Services;

/// <summary>
/// Query service for Balance entities (simple CRUD).
/// </summary>
public class BalanceQueryService : IBalanceQueryService
{
    private readonly FinanceDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<BalanceQueryService> _logger;

    public BalanceQueryService(
        FinanceDbContext context,
        IMapper mapper,
        ILogger<BalanceQueryService> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<BalanceDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting balance by ID: {BalanceId}", id);

        var balance = await _context.Balances
            .Include(b => b.Account)
            .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);

        if (balance == null)
        {
            _logger.LogWarning("Balance not found: {BalanceId}", id);
            return null;
        }

        _logger.LogInformation("Balance found: {BalanceId}", id);
        return _mapper.Map<BalanceDto>(balance);
    }

    public async Task<IEnumerable<BalanceDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting all active balances");

        var balances = await _context.Balances
            .Include(b => b.Account)
            .Where(b => b.IsActive)
            .OrderByDescending(b => b.BalanceDate)
            .ToListAsync(cancellationToken);

        _logger.LogInformation("Found {Count} active balances", balances.Count);
        return _mapper.Map<IEnumerable<BalanceDto>>(balances);
    }

    public async Task<IEnumerable<BalanceDto>> GetByAccountIdAsync(int accountId, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting balances for account ID: {AccountId}", accountId);

        var balances = await _context.Balances
            .Include(b => b.Account)
            .Where(b => b.AccountId == accountId && b.IsActive)
            .OrderByDescending(b => b.BalanceDate)
            .ToListAsync(cancellationToken);

        _logger.LogInformation("Found {Count} balances for account {AccountId}", balances.Count, accountId);
        return _mapper.Map<IEnumerable<BalanceDto>>(balances);
    }
}
