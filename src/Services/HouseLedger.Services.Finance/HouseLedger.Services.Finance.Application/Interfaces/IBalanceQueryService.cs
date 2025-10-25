using HouseLedger.Services.Finance.Application.Contracts.Balances;

namespace HouseLedger.Services.Finance.Application.Interfaces;

/// <summary>
/// Service for Balance query operations (read).
/// </summary>
public interface IBalanceQueryService
{
    Task<BalanceDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<BalanceDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<BalanceDto>> GetByAccountIdAsync(int accountId, CancellationToken cancellationToken = default);
}
