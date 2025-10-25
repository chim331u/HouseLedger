using AutoMapper;
using HouseLedger.Services.HouseThings.Application.Contracts.HouseThings;
using HouseLedger.Services.HouseThings.Application.Interfaces;
using HouseLedger.Services.HouseThings.Domain.Entities;
using HouseLedger.Services.HouseThings.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HouseLedger.Services.HouseThings.Application.Services;

/// <summary>
/// Command service implementation for HouseThing entity CRUD operations.
/// </summary>
public class HouseThingCommandService : IHouseThingCommandService
{
    private readonly HouseThingsDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<HouseThingCommandService> _logger;

    public HouseThingCommandService(
        HouseThingsDbContext context,
        IMapper mapper,
        ILogger<HouseThingCommandService> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<HouseThingDto> CreateAsync(CreateHouseThingRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating new house thing: {HouseThingName}", request.Name);

        var houseThing = _mapper.Map<HouseThing>(request);

        // Audit fields (CreatedDate, LastUpdatedDate, IsActive) will be set automatically by DbContext.UpdateAuditFields()
        _context.HouseThings.Add(houseThing);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("House thing created successfully with ID: {Id}", houseThing.Id);

        // Reload with navigation properties
        var created = await _context.HouseThings
            .Include(h => h.Room)
            .FirstOrDefaultAsync(h => h.Id == houseThing.Id, cancellationToken);

        return _mapper.Map<HouseThingDto>(created);
    }

    public async Task<HouseThingDto?> UpdateAsync(int id, UpdateHouseThingRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating house thing with ID: {Id}", id);

        var houseThing = await _context.HouseThings
            .FirstOrDefaultAsync(h => h.Id == id, cancellationToken);

        if (houseThing == null)
        {
            _logger.LogWarning("House thing with ID {Id} not found", id);
            return null;
        }

        _mapper.Map(request, houseThing);

        // LastUpdatedDate will be set automatically by DbContext.UpdateAuditFields()
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("House thing with ID {Id} updated successfully", id);

        // Reload with navigation properties
        var updated = await _context.HouseThings
            .Include(h => h.Room)
            .FirstOrDefaultAsync(h => h.Id == id, cancellationToken);

        return _mapper.Map<HouseThingDto>(updated);
    }

    public async Task<HouseThingDto> RenewAsync(int id, CreateHouseThingRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Renewing house thing with ID: {Id}", id);

        // Get the old house thing to retrieve its HistoryId
        var oldHouseThing = await _context.HouseThings
            .FirstOrDefaultAsync(h => h.Id == id, cancellationToken);

        if (oldHouseThing == null)
        {
            _logger.LogError("House thing with ID {Id} not found for renewal", id);
            throw new InvalidOperationException($"House thing with ID {id} not found");
        }

        // Soft delete the old house thing
        oldHouseThing.IsActive = false;

        // Create new house thing with the same HistoryId
        var newHouseThing = _mapper.Map<HouseThing>(request);
        newHouseThing.HistoryId = oldHouseThing.HistoryId;

        // Audit fields (CreatedDate, LastUpdatedDate, IsActive) will be set automatically by DbContext.UpdateAuditFields()
        _context.HouseThings.Add(newHouseThing);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("House thing renewed successfully. Old ID: {OldId}, New ID: {NewId}, HistoryId: {HistoryId}",
            id, newHouseThing.Id, newHouseThing.HistoryId);

        // Reload with navigation properties
        var created = await _context.HouseThings
            .Include(h => h.Room)
            .FirstOrDefaultAsync(h => h.Id == newHouseThing.Id, cancellationToken);

        return _mapper.Map<HouseThingDto>(created);
    }

    public async Task<bool> SoftDeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Soft deleting house thing with ID: {Id}", id);

        var houseThing = await _context.HouseThings
            .FirstOrDefaultAsync(h => h.Id == id, cancellationToken);

        if (houseThing == null)
        {
            _logger.LogWarning("House thing with ID {Id} not found", id);
            return false;
        }

        houseThing.IsActive = false;

        // LastUpdatedDate will be set automatically by DbContext.UpdateAuditFields()
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("House thing with ID {Id} soft deleted successfully", id);

        return true;
    }

    public async Task<bool> HardDeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        _logger.LogWarning("Hard deleting house thing with ID: {Id}", id);

        var houseThing = await _context.HouseThings
            .FirstOrDefaultAsync(h => h.Id == id, cancellationToken);

        if (houseThing == null)
        {
            _logger.LogWarning("House thing with ID {Id} not found", id);
            return false;
        }

        _context.HouseThings.Remove(houseThing);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogWarning("House thing with ID {Id} hard deleted permanently", id);

        return true;
    }
}
