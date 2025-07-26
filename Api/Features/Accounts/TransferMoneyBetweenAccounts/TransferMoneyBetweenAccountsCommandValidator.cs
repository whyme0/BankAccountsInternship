using FluentValidation;

namespace Api.Features.Accounts.TransferMoneyBetweenAccounts;

// ReSharper disable once UnusedType.Global
public class TransferMoneyBetweenAccountsCommandValidator : AbstractValidator<TransferMoneyBetweenAccountsCommand>
{
    public TransferMoneyBetweenAccountsCommandValidator()
    {
        RuleFor(x => x.SenderAccountId)
            .NotEmpty().NotEqual(Guid.Empty).WithMessage("SenderAccountId is required.");

        RuleFor(x => x.RecipientAccountId)
            .NotEmpty().NotEqual(Guid.Empty).WithMessage("RecipientAccountId is required.")
            .NotEqual(x => x.SenderAccountId).WithMessage("Sender and recipient accounts must be different.");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Amount must be greater than zero.");
    }
}