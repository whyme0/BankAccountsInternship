using Api.Abstractions;
using Api.Models;
using Api.Data;
using Api.Exceptions;
using Api.Features.Accounts.GetAccount;
using MediatR;

namespace Api.Features.Accounts.CreateAccount
{
    public class CreateAccountHandler : ICommandHandler<CreateAccountCommand, AccountDto>
    {
    private readonly IAppDbContext _context;
    private readonly IMediator _mediator;

    public CreateAccountHandler(IAppDbContext context, IMediator mediator)
    {
        _context = context;
        _mediator = mediator;
    }

    public async Task<AccountDto> Handle(CreateAccountCommand request, CancellationToken cancellationToken)
    {
        var accountOwner = _context.Clients.FirstOrDefault(c => c.Id == request.OwnerId);

        if (accountOwner == null) throw new NotFoundException(request.OwnerId.ToString());

        var account = new Account()
        {
            Id = new Guid(),
            OwnerId = accountOwner!.Id,
            Owner = accountOwner,
            Type = request.Type,
            Currency = request.Currency,
            Balance = request.Balance,
            InterestRate = request.InterestRate,
            OpenedDate = DateTime.UtcNow,
            ClosedDate = request.ClosedDate
        };

        _context.Accounts.Add(account);
        await _context.SaveChangesAsync(cancellationToken);

        return await _mediator.Send(new GetAccountQuery()
        {
            Id = account.Id
        }, cancellationToken);
    }
    }
}