using HouseLedger.Services.Salary.Application.Contracts.Salaries;

namespace HouseLedger.Services.Salary.Application.Interfaces;

/// <summary>
/// Service for querying salary data.
/// </summary>
public interface ISalaryQueryService
{
    Task<IEnumerable<SalaryDto>> GetAllAsync(bool includeInactive = false, CancellationToken cancellationToken = default);
    Task<SalaryDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<SalaryDto>> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<SalaryDto>> GetByYearAsync(string year, CancellationToken cancellationToken = default);
}
