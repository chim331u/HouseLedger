using AutoMapper;
using HouseLedger.Services.HouseThings.Application.Contracts.Rooms;
using HouseLedger.Services.HouseThings.Application.Interfaces;
using HouseLedger.Services.HouseThings.Domain.Entities;
using HouseLedger.Services.HouseThings.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HouseLedger.Services.HouseThings.Application.Services;

/// <summary>
/// Command service implementation for Room entity CRUD operations.
/// </summary>
public class RoomCommandService : IRoomCommandService
{
    private readonly HouseThingsDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<RoomCommandService> _logger;

    public RoomCommandService(
        HouseThingsDbContext context,
        IMapper mapper,
        ILogger<RoomCommandService> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<RoomDto> CreateAsync(CreateRoomRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating new room: {RoomName}", request.Name);

        var room = _mapper.Map<Room>(request);

        // Audit fields (CreatedDate, LastUpdatedDate, IsActive) will be set automatically by DbContext.UpdateAuditFields()
        _context.Rooms.Add(room);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Room created successfully with ID: {Id}", room.Id);

        return _mapper.Map<RoomDto>(room);
    }

    public async Task<RoomDto?> UpdateAsync(int id, UpdateRoomRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating room with ID: {Id}", id);

        var room = await _context.Rooms
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

        if (room == null)
        {
            _logger.LogWarning("Room with ID {Id} not found", id);
            return null;
        }

        _mapper.Map(request, room);

        // LastUpdatedDate will be set automatically by DbContext.UpdateAuditFields()
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Room with ID {Id} updated successfully", id);

        return _mapper.Map<RoomDto>(room);
    }

    public async Task<bool> SoftDeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Soft deleting room with ID: {Id}", id);

        var room = await _context.Rooms
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

        if (room == null)
        {
            _logger.LogWarning("Room with ID {Id} not found", id);
            return false;
        }

        room.IsActive = false;

        // LastUpdatedDate will be set automatically by DbContext.UpdateAuditFields()
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Room with ID {Id} soft deleted successfully", id);

        return true;
    }

    public async Task<bool> HardDeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        _logger.LogWarning("Hard deleting room with ID: {Id}", id);

        var room = await _context.Rooms
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

        if (room == null)
        {
            _logger.LogWarning("Room with ID {Id} not found", id);
            return false;
        }

        _context.Rooms.Remove(room);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogWarning("Room with ID {Id} hard deleted permanently", id);

        return true;
    }
}
