using HouseLedger.Services.Finance.Application.Contracts.Accounts;

namespace HouseLedger.Services.Finance.Application.Interfaces;

/// <summary>
/// Query service for Account entities (simple CRUD operations).
/// </summary>
public interface IAccountQueryService
{
    /// <summary>
    /// Get account by ID.
    /// </summary>
    Task<AccountDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all active accounts.
    /// </summary>
    Task<IEnumerable<AccountDto>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get accounts by bank ID.
    /// </summary>
    Task<IEnumerable<AccountDto>> GetByBankIdAsync(int bankId, CancellationToken cancellationToken = default);
}
