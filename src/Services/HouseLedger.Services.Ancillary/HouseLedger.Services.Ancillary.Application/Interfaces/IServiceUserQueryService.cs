using HouseLedger.Services.Ancillary.Application.Contracts.ServiceUsers;

namespace HouseLedger.Services.Ancillary.Application.Interfaces;

/// <summary>
/// Service for ServiceUser query operations (read-only).
/// </summary>
public interface IServiceUserQueryService
{
    Task<IEnumerable<ServiceUserDto>> GetAllAsync(bool includeInactive = false, CancellationToken cancellationToken = default);
    Task<ServiceUserDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
}
