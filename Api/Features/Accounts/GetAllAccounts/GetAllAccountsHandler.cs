using Api.Abstractions;
using Api.Data;
using Api.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Api.Features.Accounts.GetAllAccounts
{
    public class GetAllAccountsHandler : IQueryHandler<GetAllAccountsQuery, IEnumerable<AccountDto>>
    {
        private readonly IAppDbContext _context;

        public GetAllAccountsHandler(IAppDbContext context)
        {
            _context = context;
        }
        
        public async Task<IEnumerable<AccountDto>> Handle(GetAllAccountsQuery request, CancellationToken cancellationToken)
        {
            var accounts = _context.Accounts
                .Include(a => a.Owner)
                .Include(a => a.Transactions);

            return await accounts.Select(a => new AccountDto()
            {
                Id = a.Id,
                Owner = new ClientDto()
                {
                    Id = a.Owner.Id,
                    Name = a.Owner.Name
                },
                Type = a.Type,
                Currency = a.Currency,
                Balance = a.Balance,
                InterestRate = a.InterestRate,
                OpenedDate = a.OpenedDate,
                ClosedDate = a.ClosedDate,
                Transactions = a.Transactions.Select(t => new TransactionDto()
                {
                    Id = t.Id,
                    AccountId = t.AccountId,
                    CounterPartyAccountId = t.CounterPartyAccountId,
                    Amount = t.Amount,
                    Currency = t.Currency,
                    Type = t.Type,
                    Description = t.Description,
                    Date = t.Date
                }).ToList()
            }).ToListAsync();
        }
    }
}
