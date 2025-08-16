using Api.Features.Clients.BlockClient;
using Api.Features.Clients.UnblockClient;
using FluentValidation;

namespace Api.Features.Clients.UnblockClient;

public class UnblockClientCommandValidator : AbstractValidator<UnblockClientCommand>
{
    public UnblockClientCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().NotEqual(Guid.Empty).WithMessage("Id required");
    }
}
