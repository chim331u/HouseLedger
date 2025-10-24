namespace HouseLedger.Services.Salary.Application.Contracts.Salaries;

/// <summary>
/// Request to update an existing salary entry.
/// </summary>
public record UpdateSalaryRequest
{
    public int Id { get; init; }
    public decimal SalaryValue { get; init; }
    public DateTime SalaryDate { get; init; }
    public string? ReferYear { get; init; }
    public string? ReferMonth { get; init; }
    public string? FileName { get; init; }
    public int? CurrencyId { get; init; }
    public int? UserId { get; init; }
    public string? Note { get; init; }
}
