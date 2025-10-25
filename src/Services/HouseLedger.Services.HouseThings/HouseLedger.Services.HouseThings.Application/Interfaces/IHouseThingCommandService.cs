using HouseLedger.Services.HouseThings.Application.Contracts.HouseThings;

namespace HouseLedger.Services.HouseThings.Application.Interfaces;

public interface IHouseThingCommandService
{
    Task<HouseThingDto> CreateAsync(CreateHouseThingRequest request, CancellationToken cancellationToken = default);
    Task<HouseThingDto?> UpdateAsync(int id, UpdateHouseThingRequest request, CancellationToken cancellationToken = default);
    Task<HouseThingDto> RenewAsync(int id, CreateHouseThingRequest request, CancellationToken cancellationToken = default);
    Task<bool> SoftDeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> HardDeleteAsync(int id, CancellationToken cancellationToken = default);
}
