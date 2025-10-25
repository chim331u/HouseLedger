using HouseLedger.Services.Finance.Application.Contracts.Balances;

namespace HouseLedger.Services.Finance.Application.Interfaces;

/// <summary>
/// Service for Balance command operations (create, update, delete).
/// </summary>
public interface IBalanceCommandService
{
    Task<BalanceDto> CreateAsync(CreateBalanceRequest request, CancellationToken cancellationToken = default);
    Task<BalanceDto?> UpdateAsync(int id, UpdateBalanceRequest request, CancellationToken cancellationToken = default);
    Task<bool> SoftDeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> HardDeleteAsync(int id, CancellationToken cancellationToken = default);
}
