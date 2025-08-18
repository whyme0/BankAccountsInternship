using FluentValidation;
// ReSharper disable UnusedType.Global

namespace Api.Features.Clients.UnblockClient;

public class UnblockClientCommandValidator : AbstractValidator<UnblockClientCommand>
{
    public UnblockClientCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().NotEqual(Guid.Empty).WithMessage("Id required");
    }
}
