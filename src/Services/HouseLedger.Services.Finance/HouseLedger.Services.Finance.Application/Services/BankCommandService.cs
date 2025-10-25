using AutoMapper;
using HouseLedger.Services.Finance.Application.Contracts.Banks;
using HouseLedger.Services.Finance.Application.Interfaces;
using HouseLedger.Services.Finance.Domain.Entities;
using HouseLedger.Services.Finance.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HouseLedger.Services.Finance.Application.Services;

/// <summary>
/// Command service implementation for Bank entity CRUD operations.
/// </summary>
public class BankCommandService : IBankCommandService
{
    private readonly FinanceDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<BankCommandService> _logger;

    public BankCommandService(
        FinanceDbContext context,
        IMapper mapper,
        ILogger<BankCommandService> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<BankDto> CreateAsync(CreateBankRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating new bank: {BankName}", request.Name);

        var bank = _mapper.Map<Bank>(request);

        // Audit fields (CreatedDate, LastUpdatedDate, IsActive) will be set automatically by DbContext.UpdateAuditFields()
        _context.Banks.Add(bank);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Bank created successfully with ID: {Id}", bank.Id);

        return _mapper.Map<BankDto>(bank);
    }

    public async Task<BankDto?> UpdateAsync(int id, UpdateBankRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating bank with ID: {Id}", id);

        var bank = await _context.Banks
            .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);

        if (bank == null)
        {
            _logger.LogWarning("Bank with ID {Id} not found", id);
            return null;
        }

        _mapper.Map(request, bank);

        // LastUpdatedDate will be set automatically by DbContext.UpdateAuditFields()
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Bank with ID {Id} updated successfully", id);

        return _mapper.Map<BankDto>(bank);
    }

    public async Task<bool> SoftDeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Soft deleting bank with ID: {Id}", id);

        var bank = await _context.Banks
            .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);

        if (bank == null)
        {
            _logger.LogWarning("Bank with ID {Id} not found", id);
            return false;
        }

        bank.IsActive = false;

        // LastUpdatedDate will be set automatically by DbContext.UpdateAuditFields()
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Bank with ID {Id} soft deleted successfully", id);

        return true;
    }

    public async Task<bool> HardDeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        _logger.LogWarning("Hard deleting bank with ID: {Id}", id);

        var bank = await _context.Banks
            .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);

        if (bank == null)
        {
            _logger.LogWarning("Bank with ID {Id} not found", id);
            return false;
        }

        _context.Banks.Remove(bank);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogWarning("Bank with ID {Id} hard deleted permanently", id);

        return true;
    }
}
