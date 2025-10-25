using AutoMapper;
using HouseLedger.Services.Finance.Application.Contracts.Accounts;
using HouseLedger.Services.Finance.Application.Interfaces;
using HouseLedger.Services.Finance.Domain.Entities;
using HouseLedger.Services.Finance.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HouseLedger.Services.Finance.Application.Services;

/// <summary>
/// Command service implementation for Account entity CRUD operations.
/// </summary>
public class AccountCommandService : IAccountCommandService
{
    private readonly FinanceDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<AccountCommandService> _logger;

    public AccountCommandService(
        FinanceDbContext context,
        IMapper mapper,
        ILogger<AccountCommandService> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<AccountDto> CreateAsync(CreateAccountRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating new account: {AccountName}", request.Name);

        var account = _mapper.Map<Account>(request);

        // Audit fields will be set automatically by DbContext.UpdateAuditFields()
        var now = DateTime.UtcNow;
        account.CreatedDate = now;
        account.LastUpdatedDate = now;
        account.IsActive = true;

        _context.Accounts.Add(account);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Account created successfully with ID: {Id}", account.Id);

        // Reload with navigation properties
        var created = await _context.Accounts
            .Include(a => a.Bank)
            .FirstOrDefaultAsync(a => a.Id == account.Id, cancellationToken);

        return _mapper.Map<AccountDto>(created);
    }

    public async Task<AccountDto?> UpdateAsync(int id, UpdateAccountRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating account with ID: {Id}", id);

        var account = await _context.Accounts
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

        if (account == null)
        {
            _logger.LogWarning("Account with ID {Id} not found", id);
            return null;
        }

        _mapper.Map(request, account);

        // LastUpdatedDate will be set automatically by DbContext.UpdateAuditFields()
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Account with ID {Id} updated successfully", id);

        // Reload with navigation properties
        var updated = await _context.Accounts
            .Include(a => a.Bank)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

        return _mapper.Map<AccountDto>(updated);
    }

    public async Task<bool> SoftDeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Soft deleting account with ID: {Id}", id);

        var account = await _context.Accounts
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

        if (account == null)
        {
            _logger.LogWarning("Account with ID {Id} not found", id);
            return false;
        }

        account.IsActive = false;

        // LastUpdatedDate will be set automatically by DbContext.UpdateAuditFields()
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Account with ID {Id} soft deleted successfully", id);

        return true;
    }

    public async Task<bool> HardDeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        _logger.LogWarning("Hard deleting account with ID: {Id}", id);

        var account = await _context.Accounts
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

        if (account == null)
        {
            _logger.LogWarning("Account with ID {Id} not found", id);
            return false;
        }

        _context.Accounts.Remove(account);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogWarning("Account with ID {Id} hard deleted permanently", id);

        return true;
    }
}
