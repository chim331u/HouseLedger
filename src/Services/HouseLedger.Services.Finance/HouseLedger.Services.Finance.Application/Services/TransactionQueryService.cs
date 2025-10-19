using AutoMapper;
using HouseLedger.Services.Finance.Application.Contracts.Common;
using HouseLedger.Services.Finance.Application.Contracts.Transactions;
using HouseLedger.Services.Finance.Application.Interfaces;
using HouseLedger.Services.Finance.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HouseLedger.Services.Finance.Application.Services;

/// <summary>
/// Traditional query service for Transaction entities (simple read operations).
/// </summary>
public class TransactionQueryService : ITransactionQueryService
{
    private readonly FinanceDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<TransactionQueryService> _logger;

    public TransactionQueryService(
        FinanceDbContext context,
        IMapper mapper,
        ILogger<TransactionQueryService> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<TransactionDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting transaction by ID: {TransactionId}", id);

        var transaction = await _context.Transactions
            .Include(t => t.Account)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

        if (transaction == null)
        {
            _logger.LogWarning("Transaction not found: {TransactionId}", id);
            return null;
        }

        _logger.LogInformation("Transaction found: {TransactionId}", id);
        return _mapper.Map<TransactionDto>(transaction);
    }

    public async Task<PagedResult<TransactionDto>> GetByAccountIdAsync(
        int accountId,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        int page = 1,
        int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting transactions for account {AccountId}, page {Page}, pageSize {PageSize}",
            accountId, page, pageSize);

        var query = _context.Transactions
            .Include(t => t.Account)
            .Where(t => t.AccountId == accountId && t.IsActive);

        if (fromDate.HasValue)
        {
            query = query.Where(t => t.TransactionDate >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(t => t.TransactionDate <= toDate.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var transactions = await query
            .OrderByDescending(t => t.TransactionDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        _logger.LogInformation("Found {Count} of {Total} transactions for account {AccountId}",
            transactions.Count, totalCount, accountId);

        var dtos = _mapper.Map<IEnumerable<TransactionDto>>(transactions);
        return new PagedResult<TransactionDto>(dtos, totalCount, page, pageSize);
    }

    public async Task<PagedResult<TransactionDto>> GetRecentAsync(
        int page = 1,
        int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting recent transactions, page {Page}, pageSize {PageSize}", page, pageSize);

        var query = _context.Transactions
            .Include(t => t.Account)
            .Where(t => t.IsActive);

        var totalCount = await query.CountAsync(cancellationToken);

        var transactions = await query
            .OrderByDescending(t => t.TransactionDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        _logger.LogInformation("Found {Count} of {Total} recent transactions", transactions.Count, totalCount);

        var dtos = _mapper.Map<IEnumerable<TransactionDto>>(transactions);
        return new PagedResult<TransactionDto>(dtos, totalCount, page, pageSize);
    }
}
