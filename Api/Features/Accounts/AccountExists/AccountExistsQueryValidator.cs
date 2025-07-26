using FluentValidation;

namespace Api.Features.Accounts.AccountExists
{
    public class AccountExistsQueryValidator : AbstractValidator<AccountExistsQuery>
    {
        public AccountExistsQueryValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().NotEqual(Guid.Empty).WithMessage("AccountId is required.");

            RuleFor(x => x.OwnerId)
                .NotEmpty().NotEqual(Guid.Empty).WithMessage("OwnerId is required.");
        }
    }
}
