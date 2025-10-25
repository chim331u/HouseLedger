using HouseLedger.Services.Finance.Application.Contracts.Accounts;

namespace HouseLedger.Services.Finance.Application.Interfaces;

/// <summary>
/// Service for Account command operations (create, update, delete).
/// </summary>
public interface IAccountCommandService
{
    Task<AccountDto> CreateAsync(CreateAccountRequest request, CancellationToken cancellationToken = default);
    Task<AccountDto?> UpdateAsync(int id, UpdateAccountRequest request, CancellationToken cancellationToken = default);
    Task<bool> SoftDeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> HardDeleteAsync(int id, CancellationToken cancellationToken = default);
}
