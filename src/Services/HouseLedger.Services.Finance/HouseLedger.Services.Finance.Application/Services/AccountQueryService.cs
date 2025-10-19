using AutoMapper;
using HouseLedger.Services.Finance.Application.Contracts.Accounts;
using HouseLedger.Services.Finance.Application.Interfaces;
using HouseLedger.Services.Finance.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HouseLedger.Services.Finance.Application.Services;

/// <summary>
/// Traditional query service for Account entities (simple CRUD).
/// </summary>
public class AccountQueryService : IAccountQueryService
{
    private readonly FinanceDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<AccountQueryService> _logger;

    public AccountQueryService(
        FinanceDbContext context,
        IMapper mapper,
        ILogger<AccountQueryService> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<AccountDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting account by ID: {AccountId}", id);

        var account = await _context.Accounts
            .Include(a => a.Bank)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

        if (account == null)
        {
            _logger.LogWarning("Account not found: {AccountId}", id);
            return null;
        }

        _logger.LogInformation("Account found: {AccountId} - {AccountName}", id, account.Name);
        return _mapper.Map<AccountDto>(account);
    }

    public async Task<IEnumerable<AccountDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting all active accounts");

        var accounts = await _context.Accounts
            .Include(a => a.Bank)
            .Where(a => a.IsActive)
            .OrderBy(a => a.Name)
            .ToListAsync(cancellationToken);

        _logger.LogInformation("Found {Count} active accounts", accounts.Count);
        return _mapper.Map<IEnumerable<AccountDto>>(accounts);
    }

    public async Task<IEnumerable<AccountDto>> GetByBankIdAsync(int bankId, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting accounts for bank: {BankId}", bankId);

        var accounts = await _context.Accounts
            .Include(a => a.Bank)
            .Where(a => a.BankId == bankId && a.IsActive)
            .OrderBy(a => a.Name)
            .ToListAsync(cancellationToken);

        _logger.LogInformation("Found {Count} accounts for bank {BankId}", accounts.Count, bankId);
        return _mapper.Map<IEnumerable<AccountDto>>(accounts);
    }
}
