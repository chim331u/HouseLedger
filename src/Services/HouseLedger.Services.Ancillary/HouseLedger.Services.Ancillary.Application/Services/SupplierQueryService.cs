using AutoMapper;
using HouseLedger.Services.Ancillary.Application.Contracts.Suppliers;
using HouseLedger.Services.Ancillary.Application.Interfaces;
using HouseLedger.Services.Ancillary.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HouseLedger.Services.Ancillary.Application.Services;

/// <summary>
/// Query service implementation for Supplier entities.
/// </summary>
public class SupplierQueryService : ISupplierQueryService
{
    private readonly AncillaryDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<SupplierQueryService> _logger;

    public SupplierQueryService(
        AncillaryDbContext context,
        IMapper mapper,
        ILogger<SupplierQueryService> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<SupplierDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting supplier with ID: {Id}", id);

        var supplier = await _context.Suppliers
            .Where(s => s.Id == id && s.IsActive)
            .FirstOrDefaultAsync(cancellationToken);

        if (supplier == null)
        {
            _logger.LogWarning("Supplier with ID {Id} not found", id);
            return null;
        }

        return _mapper.Map<SupplierDto>(supplier);
    }

    public async Task<IEnumerable<SupplierDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting all active suppliers");

        var suppliers = await _context.Suppliers
            .Where(s => s.IsActive)
            .OrderBy(s => s.Name)
            .ToListAsync(cancellationToken);

        _logger.LogInformation("Retrieved {Count} suppliers", suppliers.Count);

        return _mapper.Map<IEnumerable<SupplierDto>>(suppliers);
    }

    public async Task<IEnumerable<SupplierDto>> GetByTypeAsync(string type, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting suppliers by type: {Type}", type);

        var suppliers = await _context.Suppliers
            .Where(s => s.Type == type && s.IsActive)
            .OrderBy(s => s.Name)
            .ToListAsync(cancellationToken);

        _logger.LogInformation("Retrieved {Count} suppliers of type {Type}", suppliers.Count, type);

        return _mapper.Map<IEnumerable<SupplierDto>>(suppliers);
    }
}
