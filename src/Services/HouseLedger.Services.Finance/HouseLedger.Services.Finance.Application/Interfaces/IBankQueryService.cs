using HouseLedger.Services.Finance.Application.Contracts.Banks;

namespace HouseLedger.Services.Finance.Application.Interfaces;

/// <summary>
/// Query service for Bank entities.
/// </summary>
public interface IBankQueryService
{
    Task<BankDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<BankDto>> GetAllAsync(CancellationToken cancellationToken = default);
}
