using FluentValidation;

namespace Api.Features.Accounts.CreateAccount;

// ReSharper disable once UnusedType.Global
public class CreateAccountCommandValidator : AbstractValidator<CreateAccountCommand>
{
    public CreateAccountCommandValidator()
    {
        RuleFor(x => x.OwnerId)
            .NotEmpty().NotEqual(Guid.Empty).WithMessage("OwnerId is required.");

        RuleFor(x => x.Type)
            .IsInEnum().WithMessage("Invalid account type.");

        RuleFor(x => x.Currency)
            .NotEmpty().WithMessage("Currency is required.")
            .Length(3).WithMessage("Currency must be a 3-letter ISO code.");

        RuleFor(x => x.Balance)
            .GreaterThanOrEqualTo(0).WithMessage("Balance cannot be negative.");

        RuleFor(x => x.InterestRate)
            .GreaterThanOrEqualTo(0).WithMessage("Interest rate cannot be negative.")
            .LessThanOrEqualTo(100).WithMessage("Interest rate cannot exceed 100%.");

        RuleFor(x => x.ClosedDate)
            .Must(date => date == null || date > DateTime.UtcNow)
            .WithMessage("ClosedDate must be in the future if specified.");
    }
}