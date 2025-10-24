using HouseLedger.Services.Ancillary.Application.Contracts.ServiceUsers;

namespace HouseLedger.Services.Ancillary.Application.Interfaces;

/// <summary>
/// Service for ServiceUser command operations (create, update, delete).
/// </summary>
public interface IServiceUserCommandService
{
    Task<ServiceUserDto> CreateAsync(CreateServiceUserRequest request, CancellationToken cancellationToken = default);
    Task<ServiceUserDto?> UpdateAsync(int id, UpdateServiceUserRequest request, CancellationToken cancellationToken = default);
    Task<bool> SoftDeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> HardDeleteAsync(int id, CancellationToken cancellationToken = default);
}
