using Api.Abstractions;
using Api.Data;
using Api.Exceptions;
using Api.Models;
using Api.Presentation.MessageEvents;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Text.Json;

namespace Api.Features.Transactions.CreateTransaction;

public class CreateTransactionHandler(IAppDbContext context)
    : ICommandHandler<CreateTransactionCommand, Transaction>
{
    public async Task<Transaction> Handle(CreateTransactionCommand request, CancellationToken cancellationToken)
    {
        var account = await context.Accounts.FirstOrDefaultAsync(a => a.Id == request.AccountId, cancellationToken);
        var accountTransactionId = Guid.NewGuid();
        DateTime occuredAt;
        Outbox outbox;

        if (account == null) throw new NotFoundException();

        switch (request.Type)
        {
            case TransactionType.Debit:
                account.Balance += request.Amount;
                occuredAt = DateTime.UtcNow;
                outbox = new Outbox
                {
                    Id = Guid.NewGuid(),
                    OccurredAt = occuredAt,
                    Type = "MoneyDebited",
                    RoutingKey = "money.transfer.debited",
                    Payload = JsonSerializer.Serialize(new MoneyDebited
                    {
                        EventId = Guid.NewGuid(),
                        OccuredAt = occuredAt,
                        AccountId = request.AccountId,
                        Amount = request.Amount,
                        Currency = request.Currency,
                        OperationId = accountTransactionId,
                        Reason = request.Description ?? string.Empty
                    })
                };
                break;
            case TransactionType.Credit:
                account.Balance -= request.Amount;
                occuredAt = DateTime.UtcNow;
                outbox = new Outbox
                {
                    Id = Guid.NewGuid(),
                    OccurredAt = occuredAt,
                    Type = "MoneyCredited",
                    RoutingKey = "money.transfer.credited",
                    Payload = JsonSerializer.Serialize(new MoneyCredited
                    {
                        EventId = Guid.NewGuid(),
                        OccuredAt = occuredAt,
                        AccountId = request.AccountId,
                        Amount = request.Amount,
                        Currency = request.Currency,
                        OperationId = accountTransactionId
                    })
                };
                break;
            default:
                throw new BadRequestException($"Wrong transaction type ({request.Type})");
        }


        var accountTransaction = new Transaction
        {
            Id = accountTransactionId,
            AccountId = request.AccountId,
            CounterPartyAccountId = request.CounterPartyAccountId,
            Amount = request.Amount,
            Currency = request.Currency,
            Type = request.Type,
            Description = request.Description,
            Date = DateTime.UtcNow
        };

        
        context.Outbox.Add(outbox);
        context.Accounts.Update(account);
        context.Transactions.Add(accountTransaction);
        await context.SaveChangesAsync(cancellationToken);

        return accountTransaction;
    }
}