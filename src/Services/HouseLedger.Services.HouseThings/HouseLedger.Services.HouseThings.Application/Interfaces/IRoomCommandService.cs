using HouseLedger.Services.HouseThings.Application.Contracts.Rooms;

namespace HouseLedger.Services.HouseThings.Application.Interfaces;

public interface IRoomCommandService
{
    Task<RoomDto> CreateAsync(CreateRoomRequest request, CancellationToken cancellationToken = default);
    Task<RoomDto?> UpdateAsync(int id, UpdateRoomRequest request, CancellationToken cancellationToken = default);
    Task<bool> SoftDeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> HardDeleteAsync(int id, CancellationToken cancellationToken = default);
}
