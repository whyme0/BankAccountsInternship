using FluentValidation;

namespace Api.Features.Accounts.GetAccount;

// ReSharper disable once UnusedType.Global
public class GetAccountQueryValidator : AbstractValidator<GetAccountQuery>
{
    public GetAccountQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().NotEqual(Guid.Empty).WithMessage("AccountId is required.");
    }
}