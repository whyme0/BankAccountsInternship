using Api.Abstractions;
using Api.Data;
using Api.Exceptions;
using MediatR;

namespace Api.Features.Accounts.DeleteAccount;

public class DeleteAccountHandler(IAppDbContext context) : ICommandHandler<DeleteAccountCommand, Unit>
{
    public async Task<Unit> Handle(DeleteAccountCommand request, CancellationToken cancellationToken)
    {
        var account = context.Accounts.FirstOrDefault(a => a.Id == request.Id);

        if (account == null)
            throw new NotFoundException(request.Id.ToString());

        context.Accounts.Remove(account);
        await context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}