namespace HouseLedger.Services.Salary.Application.Contracts.Salaries;

/// <summary>
/// DTO for Salary entity.
/// </summary>
public record SalaryDto
{
    public int Id { get; init; }
    public decimal SalaryValue { get; init; }
    public decimal SalaryValueEur { get; init; }
    public DateTime SalaryDate { get; init; }
    public string? ReferYear { get; init; }
    public string? ReferMonth { get; init; }
    public string? FileName { get; init; }
    public decimal ExchangeRate { get; init; }
    public int? CurrencyId { get; init; }
    public string? CurrencyName { get; init; }
    public string? CurrencyCode { get; init; }
    public int? UserId { get; init; }
    public string? UserName { get; init; }
    public string? Note { get; init; }
    public DateTime CreatedDate { get; init; }
    public DateTime LastUpdatedDate { get; init; }
    public bool IsActive { get; init; }
}
