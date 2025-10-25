using AutoMapper;
using HouseLedger.Services.HouseThings.Application.Contracts.Rooms;
using HouseLedger.Services.HouseThings.Application.Interfaces;
using HouseLedger.Services.HouseThings.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HouseLedger.Services.HouseThings.Application.Services;

/// <summary>
/// Query service for Room entities (simple CRUD).
/// </summary>
public class RoomQueryService : IRoomQueryService
{
    private readonly HouseThingsDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<RoomQueryService> _logger;

    public RoomQueryService(
        HouseThingsDbContext context,
        IMapper mapper,
        ILogger<RoomQueryService> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<RoomDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting room by ID: {RoomId}", id);

        var room = await _context.Rooms
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

        if (room == null)
        {
            _logger.LogWarning("Room not found: {RoomId}", id);
            return null;
        }

        _logger.LogInformation("Room found: {RoomId} - {RoomName}", id, room.Name);
        return _mapper.Map<RoomDto>(room);
    }

    public async Task<IEnumerable<RoomDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting all active rooms");

        var rooms = await _context.Rooms
            .Where(r => r.IsActive)
            .OrderByDescending(r => r.CreatedDate)
            .ToListAsync(cancellationToken);

        _logger.LogInformation("Found {Count} active rooms", rooms.Count);
        return _mapper.Map<IEnumerable<RoomDto>>(rooms);
    }
}
