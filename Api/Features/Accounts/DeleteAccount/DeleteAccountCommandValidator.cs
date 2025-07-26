using FluentValidation;

namespace Api.Features.Accounts.DeleteAccount;

// ReSharper disable once UnusedType.Global
public class DeleteAccountCommandValidator : AbstractValidator<DeleteAccountCommand>
{
    public DeleteAccountCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Account id is required")
            .NotEqual(Guid.Empty).WithMessage("Account id cannot be empty.");
    }
}