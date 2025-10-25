using AutoMapper;
using HouseLedger.Services.Finance.Application.Contracts.Banks;
using HouseLedger.Services.Finance.Application.Interfaces;
using HouseLedger.Services.Finance.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HouseLedger.Services.Finance.Application.Services;

/// <summary>
/// Query service for Bank entities (simple CRUD).
/// </summary>
public class BankQueryService : IBankQueryService
{
    private readonly FinanceDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<BankQueryService> _logger;

    public BankQueryService(
        FinanceDbContext context,
        IMapper mapper,
        ILogger<BankQueryService> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<BankDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting bank by ID: {BankId}", id);

        var bank = await _context.Banks
            .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);

        if (bank == null)
        {
            _logger.LogWarning("Bank not found: {BankId}", id);
            return null;
        }

        _logger.LogInformation("Bank found: {BankId} - {BankName}", id, bank.Name);
        return _mapper.Map<BankDto>(bank);
    }

    public async Task<IEnumerable<BankDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting all active banks");

        var banks = await _context.Banks
            .Where(b => b.IsActive)
            .OrderByDescending(b => b.CreatedDate)
            .ToListAsync(cancellationToken);

        _logger.LogInformation("Found {Count} active banks", banks.Count);
        return _mapper.Map<IEnumerable<BankDto>>(banks);
    }
}
