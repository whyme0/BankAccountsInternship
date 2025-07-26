using FluentValidation;

namespace Api.Features.Accounts.UpdateAccount
{
    public class UpdateAccountCommandValidator : AbstractValidator<UpdateAccountCommand>
    {
        public UpdateAccountCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Account Id is required.")
                .NotEqual(Guid.Empty).WithMessage("Account Id cannot be empty guid.");

            When(x => x.InterestRate.HasValue, () =>
            {
                RuleFor(x => x.InterestRate!.Value)
                    .GreaterThanOrEqualTo(0).WithMessage("Interest rate cannot be negative.")
                    .LessThanOrEqualTo(100).WithMessage("Interest rate cannot exceed 100%.");
            });

            When(x => x.ClosedDate.HasValue, () =>
            {
                RuleFor(x => x.ClosedDate)
                    .GreaterThan(DateTime.UtcNow).WithMessage("Closed date must be in the future.");
            });
        }
    }
}
