using AutoMapper;
using HouseLedger.Services.Finance.Application.Contracts.Balances;
using HouseLedger.Services.Finance.Application.Interfaces;
using HouseLedger.Services.Finance.Domain.Entities;
using HouseLedger.Services.Finance.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HouseLedger.Services.Finance.Application.Services;

/// <summary>
/// Command service implementation for Balance entity CRUD operations.
/// </summary>
public class BalanceCommandService : IBalanceCommandService
{
    private readonly FinanceDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<BalanceCommandService> _logger;

    public BalanceCommandService(
        FinanceDbContext context,
        IMapper mapper,
        ILogger<BalanceCommandService> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<BalanceDto> CreateAsync(CreateBalanceRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating new balance for account: {AccountId} on {BalanceDate}", request.AccountId, request.BalanceDate);

        var balance = _mapper.Map<Balance>(request);

        // Audit fields (CreatedDate, LastUpdatedDate, IsActive) will be set automatically by DbContext.UpdateAuditFields()
        _context.Balances.Add(balance);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Balance created successfully with ID: {Id}", balance.Id);

        // Reload with navigation properties
        var created = await _context.Balances
            .Include(b => b.Account)
            .FirstOrDefaultAsync(b => b.Id == balance.Id, cancellationToken);

        return _mapper.Map<BalanceDto>(created);
    }

    public async Task<BalanceDto?> UpdateAsync(int id, UpdateBalanceRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating balance with ID: {Id}", id);

        var balance = await _context.Balances
            .Include(b => b.Account)
            .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);

        if (balance == null)
        {
            _logger.LogWarning("Balance with ID {Id} not found", id);
            return null;
        }

        _mapper.Map(request, balance);

        // LastUpdatedDate will be set automatically by DbContext.UpdateAuditFields()
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Balance with ID {Id} updated successfully", id);

        return _mapper.Map<BalanceDto>(balance);
    }

    public async Task<bool> SoftDeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Soft deleting balance with ID: {Id}", id);

        var balance = await _context.Balances
            .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);

        if (balance == null)
        {
            _logger.LogWarning("Balance with ID {Id} not found", id);
            return false;
        }

        balance.IsActive = false;

        // LastUpdatedDate will be set automatically by DbContext.UpdateAuditFields()
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Balance with ID {Id} soft deleted successfully", id);

        return true;
    }

    public async Task<bool> HardDeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        _logger.LogWarning("Hard deleting balance with ID: {Id}", id);

        var balance = await _context.Balances
            .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);

        if (balance == null)
        {
            _logger.LogWarning("Balance with ID {Id} not found", id);
            return false;
        }

        _context.Balances.Remove(balance);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogWarning("Balance with ID {Id} hard deleted permanently", id);

        return true;
    }
}
