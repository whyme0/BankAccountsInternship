using Api.Abstractions;
using Api.Models;
using Api.Data;
using Api.Exceptions;
using Api.Features.Accounts.GetAccount;
using MediatR;
// ReSharper disable PreferConcreteValueOverDefault

namespace Api.Features.Accounts.CreateAccount;

public class CreateAccountHandler(IAppDbContext context, IMediator mediator) : ICommandHandler<CreateAccountCommand, AccountDto>
{
    public async Task<AccountDto> Handle(CreateAccountCommand request, CancellationToken cancellationToken)
    {
        var accountOwner = context.Clients.FirstOrDefault(c => c.Id == request.OwnerId);

        if (accountOwner == null) throw new NotFoundException(request.OwnerId.ToString());

        var account = new Account
        {
            Id = new Guid(),
            OwnerId = accountOwner.Id,
            Owner = accountOwner,
            Type = request.Type,
            Currency = request.Currency,
            Balance = request.Balance,
            InterestRate = request.InterestRate,
            OpenedDate = DateTime.UtcNow,
            ClosedDate = request.ClosedDate
        };

        context.Accounts.Add(account);
        await context.SaveChangesAsync(cancellationToken);

        return await mediator.Send(new GetAccountQuery
        {
            Id = account.Id
        }, cancellationToken);
    }
}