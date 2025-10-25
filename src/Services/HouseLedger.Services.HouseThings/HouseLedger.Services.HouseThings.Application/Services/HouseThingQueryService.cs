using AutoMapper;
using HouseLedger.Services.HouseThings.Application.Contracts.HouseThings;
using HouseLedger.Services.HouseThings.Application.Interfaces;
using HouseLedger.Services.HouseThings.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HouseLedger.Services.HouseThings.Application.Services;

/// <summary>
/// Query service for HouseThing entities (simple CRUD).
/// </summary>
public class HouseThingQueryService : IHouseThingQueryService
{
    private readonly HouseThingsDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<HouseThingQueryService> _logger;

    public HouseThingQueryService(
        HouseThingsDbContext context,
        IMapper mapper,
        ILogger<HouseThingQueryService> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<HouseThingDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting house thing by ID: {HouseThingId}", id);

        var houseThing = await _context.HouseThings
            .Include(h => h.Room)
            .FirstOrDefaultAsync(h => h.Id == id, cancellationToken);

        if (houseThing == null)
        {
            _logger.LogWarning("House thing not found: {HouseThingId}", id);
            return null;
        }

        _logger.LogInformation("House thing found: {HouseThingId} - {HouseThingName}", id, houseThing.Name);
        return _mapper.Map<HouseThingDto>(houseThing);
    }

    public async Task<IEnumerable<HouseThingDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting all active house things");

        var houseThings = await _context.HouseThings
            .Include(h => h.Room)
            .Where(h => h.IsActive)
            .OrderByDescending(h => h.PurchaseDate)
            .ToListAsync(cancellationToken);

        _logger.LogInformation("Found {Count} active house things", houseThings.Count);
        return _mapper.Map<IEnumerable<HouseThingDto>>(houseThings);
    }

    public async Task<IEnumerable<HouseThingDto>> GetByRoomIdAsync(int roomId, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting house things for room ID: {RoomId}", roomId);

        var houseThings = await _context.HouseThings
            .Include(h => h.Room)
            .Where(h => h.RoomId == roomId && h.IsActive)
            .OrderByDescending(h => h.PurchaseDate)
            .ToListAsync(cancellationToken);

        _logger.LogInformation("Found {Count} active house things for room ID: {RoomId}", houseThings.Count, roomId);
        return _mapper.Map<IEnumerable<HouseThingDto>>(houseThings);
    }

    public async Task<IEnumerable<HouseThingDto>> GetHistoryAsync(int historyId, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting house thing history for history ID: {HistoryId}", historyId);

        var houseThings = await _context.HouseThings
            .Include(h => h.Room)
            .Where(h => h.HistoryId == historyId)
            .OrderByDescending(h => h.PurchaseDate)
            .ToListAsync(cancellationToken);

        _logger.LogInformation("Found {Count} house things in history for history ID: {HistoryId}", houseThings.Count, historyId);
        return _mapper.Map<IEnumerable<HouseThingDto>>(houseThings);
    }
}
