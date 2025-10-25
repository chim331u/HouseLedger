using HouseLedger.Services.Finance.Application.Contracts.Banks;

namespace HouseLedger.Services.Finance.Application.Interfaces;

/// <summary>
/// Service for Bank command operations (create, update, delete).
/// </summary>
public interface IBankCommandService
{
    Task<BankDto> CreateAsync(CreateBankRequest request, CancellationToken cancellationToken = default);
    Task<BankDto?> UpdateAsync(int id, UpdateBankRequest request, CancellationToken cancellationToken = default);
    Task<bool> SoftDeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> HardDeleteAsync(int id, CancellationToken cancellationToken = default);
}
