using FluentValidation;

namespace Api.Features.Accounts.DeleteAccount
{
    public class DeleteAccountCommandValidator : AbstractValidator<DeleteAccountCommand>
    {
        public DeleteAccountCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().NotEqual(Guid.Empty).WithMessage("AccountId is required.");
        }
    }
}
