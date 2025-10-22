using AutoMapper;
using HouseLedger.Services.Ancillary.Application.Contracts.Suppliers;
using HouseLedger.Services.Ancillary.Application.Interfaces;
using HouseLedger.Services.Ancillary.Domain.Entities;
using HouseLedger.Services.Ancillary.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HouseLedger.Services.Ancillary.Application.Services;

/// <summary>
/// Command service implementation for Supplier entity CRUD operations.
/// </summary>
public class SupplierCommandService : ISupplierCommandService
{
    private readonly AncillaryDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<SupplierCommandService> _logger;

    public SupplierCommandService(
        AncillaryDbContext context,
        IMapper mapper,
        ILogger<SupplierCommandService> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<SupplierDto> CreateAsync(CreateSupplierRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating new supplier: {SupplierName}", request.Name);

        var supplier = _mapper.Map<Supplier>(request);
        supplier.CreatedDate = DateTime.UtcNow;
        supplier.LastUpdatedDate = DateTime.UtcNow;
        supplier.IsActive = true;

        _context.Suppliers.Add(supplier);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Supplier created successfully with ID: {Id}", supplier.Id);

        return _mapper.Map<SupplierDto>(supplier);
    }

    public async Task<SupplierDto?> UpdateAsync(int id, UpdateSupplierRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating supplier with ID: {Id}", id);

        var supplier = await _context.Suppliers
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

        if (supplier == null)
        {
            _logger.LogWarning("Supplier with ID {Id} not found", id);
            return null;
        }

        _mapper.Map(request, supplier);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Supplier with ID {Id} updated successfully", id);

        return _mapper.Map<SupplierDto>(supplier);
    }

    public async Task<bool> SoftDeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Soft deleting supplier with ID: {Id}", id);

        var supplier = await _context.Suppliers
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

        if (supplier == null)
        {
            _logger.LogWarning("Supplier with ID {Id} not found", id);
            return false;
        }

        supplier.IsActive = false;
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Supplier with ID {Id} soft deleted successfully", id);

        return true;
    }

    public async Task<bool> HardDeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        _logger.LogWarning("Hard deleting supplier with ID: {Id}", id);

        var supplier = await _context.Suppliers
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

        if (supplier == null)
        {
            _logger.LogWarning("Supplier with ID {Id} not found", id);
            return false;
        }

        _context.Suppliers.Remove(supplier);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogWarning("Supplier with ID {Id} hard deleted permanently", id);

        return true;
    }
}
