using Api.Abstractions;
using Api.Data;
using Api.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Api.Features.Accounts.GetAccount;

public class GetAccountHandler(IAppDbContext context) : IQueryHandler<GetAccountQuery, AccountDto>
{
    public async Task<AccountDto> Handle(GetAccountQuery request, CancellationToken cancellationToken)
    {
        var account = await context.Accounts
            .Include(a => a.Owner)
            .Include(a => a.Transactions).FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);
            
        if (account == null) throw new NotFoundException();

        return new AccountDto
        {
            Id = account.Id,
            Balance = account.Balance,
            ClosedDate = account.ClosedDate,
            Currency = account.Currency,
            InterestRate = account.InterestRate,
            OpenedDate = account.OpenedDate,
            Owner = new ClientDto
            {
                Id = account.Owner.Id,
                Name = account.Owner.Name
            },
            Transactions = account.Transactions.Select(t => new TransactionDto
            {
                AccountId = account.Id,
                Amount = t.Amount,
                CounterPartyAccountId = t.CounterPartyAccountId,
                Currency = t.Currency,
                Date = t.Date,
                Description = t.Description,
                Id = t.Id,
                Type = t.Type
            }).ToList(),
            Type = account.Type
        };
    }
}