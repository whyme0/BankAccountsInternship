using Api.Abstractions;
using Api.Data;
using Api.Exceptions;
using Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Api.Features.Transactions.CreateTransaction
{
    public class CreateTransactionHandler : ICommandHandler<CreateTransactionCommand, Transaction>
    {
        private readonly IAppDbContext _context;

        public CreateTransactionHandler(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<Transaction> Handle(CreateTransactionCommand request, CancellationToken cancellationToken)
        {
            var account = await _context.Accounts.FirstOrDefaultAsync(a => a.Id == request.AccountId, cancellationToken);

            if (account == null) throw new NotFoundException(request.AccountId.ToString());

            var transaction = new Transaction
            {
                Id = Guid.NewGuid(),
                AccountId = request.AccountId,
                CounterPartyAccountId = request.CounterPartyAccountId,
                Amount = request.Amount,
                Currency = request.Currency,
                Type = request.Type,
                Description = request.Description,
                Date = request.Date
            };

            account.Balance += request.Amount;

            _context.Accounts.Update(account);
            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync(cancellationToken);

            return transaction;
        }
    }
}
