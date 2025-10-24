using HouseLedger.Core.Domain.Common;

namespace HouseLedger.Services.Salary.Domain.Entities;

/// <summary>
/// Salary entity representing an employee salary payment.
/// Tracks salary amounts in original currency and EUR equivalent.
/// </summary>
public class Salary : AuditableEntity
{
    /// <summary>
    /// Salary amount in the original currency.
    /// </summary>
    public decimal SalaryValue { get; set; }

    /// <summary>
    /// Salary amount converted to EUR (calculated field: SalaryValue / ExchangeRate).
    /// </summary>
    public decimal SalaryValueEur { get; set; }

    /// <summary>
    /// Date when the salary was paid/received.
    /// </summary>
    public DateTime SalaryDate { get; set; }

    /// <summary>
    /// Reference year for the salary period (e.g., "2024").
    /// </summary>
    public string? ReferYear { get; set; }

    /// <summary>
    /// Reference month for the salary period (e.g., "January", "01").
    /// </summary>
    public string? ReferMonth { get; set; }

    /// <summary>
    /// Optional filename for associated salary slip document.
    /// </summary>
    public string? FileName { get; set; }

    /// <summary>
    /// Exchange rate used for currency conversion to EUR.
    /// </summary>
    public decimal ExchangeRate { get; set; }

    /// <summary>
    /// Foreign key to Currency.
    /// </summary>
    public int? CurrencyId { get; set; }

    /// <summary>
    /// Foreign key to User (ServiceUser from Ancillary).
    /// </summary>
    public int? UserId { get; set; }

    // Note: Navigation properties for Currency and User are not included in Domain
    // They are managed at the Infrastructure layer through EF Core configurations
    // Note property is inherited from AuditableEntity
}
