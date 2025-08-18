using FluentValidation;
// ReSharper disable UnusedType.Global

namespace Api.Features.Clients.BlockClient;

public class BlockClientCommandValidator : AbstractValidator<BlockClientCommand>
{
    public BlockClientCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().NotEqual(Guid.Empty).WithMessage("Id required");
    }
}