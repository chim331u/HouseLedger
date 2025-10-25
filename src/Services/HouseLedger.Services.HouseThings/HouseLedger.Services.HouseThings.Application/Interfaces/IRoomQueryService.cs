using HouseLedger.Services.HouseThings.Application.Contracts.Rooms;

namespace HouseLedger.Services.HouseThings.Application.Interfaces;

public interface IRoomQueryService
{
    Task<RoomDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<RoomDto>> GetAllAsync(CancellationToken cancellationToken = default);
}
