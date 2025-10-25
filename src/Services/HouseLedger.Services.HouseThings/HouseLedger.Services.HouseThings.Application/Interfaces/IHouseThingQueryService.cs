using HouseLedger.Services.HouseThings.Application.Contracts.HouseThings;

namespace HouseLedger.Services.HouseThings.Application.Interfaces;

public interface IHouseThingQueryService
{
    Task<HouseThingDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<HouseThingDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<HouseThingDto>> GetByRoomIdAsync(int roomId, CancellationToken cancellationToken = default);
    Task<IEnumerable<HouseThingDto>> GetHistoryAsync(int historyId, CancellationToken cancellationToken = default);
}
