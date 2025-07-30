using Api.Abstractions;
using Api.Data;
using Api.Exceptions;
using Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Api.Features.Transactions.CreateTransaction;

public class CreateTransactionHandler(IAppDbContext context)
    : ICommandHandler<CreateTransactionCommand, Transaction>
{
    public async Task<Transaction> Handle(CreateTransactionCommand request, CancellationToken cancellationToken)
    {
        var account = await context.Accounts.FirstOrDefaultAsync(a => a.Id == request.AccountId, cancellationToken);

        if (account == null) throw new NotFoundException();

        switch (request.Type)
        {
            case TransactionType.Debit:
                account.Balance += request.Amount;
                break;
            case TransactionType.Credit:
                account.Balance -= request.Amount;
                break;
            default:
                throw new BadRequestException($"Wrong transaction type ({request.Type})");
        }

        var transaction = new Transaction
        {
            Id = Guid.NewGuid(),
            AccountId = request.AccountId,
            CounterPartyAccountId = request.CounterPartyAccountId,
            Amount = request.Amount,
            Currency = request.Currency,
            Type = request.Type,
            Description = request.Description,
            Date = DateTime.UtcNow
        };

        context.Accounts.Update(account);
        context.Transactions.Add(transaction);
        await context.SaveChangesAsync(cancellationToken);

        return transaction;
    }
}