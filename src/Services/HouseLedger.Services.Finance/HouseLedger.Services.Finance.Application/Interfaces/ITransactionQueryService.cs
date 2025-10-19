using HouseLedger.Services.Finance.Application.Contracts.Common;
using HouseLedger.Services.Finance.Application.Contracts.Transactions;

namespace HouseLedger.Services.Finance.Application.Interfaces;

/// <summary>
/// Query service for Transaction entities (simple read operations).
/// </summary>
public interface ITransactionQueryService
{
    /// <summary>
    /// Get transaction by ID.
    /// </summary>
    Task<TransactionDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get transactions for an account with optional date range and paging.
    /// </summary>
    Task<PagedResult<TransactionDto>> GetByAccountIdAsync(
        int accountId,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        int page = 1,
        int pageSize = 50,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get recent transactions across all accounts.
    /// </summary>
    Task<PagedResult<TransactionDto>> GetRecentAsync(
        int page = 1,
        int pageSize = 50,
        CancellationToken cancellationToken = default);
}
