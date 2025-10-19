using FluentValidation;

namespace HouseLedger.Services.Finance.Application.Features.Transactions.CreateTransaction;

/// <summary>
/// Validator for CreateTransactionCommand.
/// Uses FluentValidation for clear, testable validation rules.
/// </summary>
public class CreateTransactionValidator : AbstractValidator<CreateTransactionCommand>
{
    public CreateTransactionValidator()
    {
        RuleFor(x => x.TransactionDate)
            .NotEmpty()
            .WithMessage("Transaction date is required")
            .LessThanOrEqualTo(DateTime.Now.AddDays(1))
            .WithMessage("Transaction date cannot be in the future");

        RuleFor(x => x.Amount)
            .NotEqual(0)
            .WithMessage("Amount cannot be zero");

        RuleFor(x => x.AccountId)
            .GreaterThan(0)
            .WithMessage("Valid account ID is required");

        RuleFor(x => x.Description)
            .MaximumLength(500)
            .WithMessage("Description cannot exceed 500 characters");

        RuleFor(x => x.CategoryName)
            .MaximumLength(100)
            .WithMessage("Category name cannot exceed 100 characters");

        RuleFor(x => x.Note)
            .MaximumLength(1000)
            .WithMessage("Note cannot exceed 1000 characters");
    }
}
