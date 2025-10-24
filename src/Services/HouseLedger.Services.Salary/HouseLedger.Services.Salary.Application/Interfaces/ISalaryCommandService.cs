using HouseLedger.Services.Salary.Application.Contracts.Salaries;

namespace HouseLedger.Services.Salary.Application.Interfaces;

/// <summary>
/// Service for salary command operations (create, update, delete).
/// </summary>
public interface ISalaryCommandService
{
    Task<SalaryDto> CreateAsync(CreateSalaryRequest request, CancellationToken cancellationToken = default);
    Task<SalaryDto?> UpdateAsync(int id, UpdateSalaryRequest request, CancellationToken cancellationToken = default);
    Task<bool> SoftDeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> HardDeleteAsync(int id, CancellationToken cancellationToken = default);
}
