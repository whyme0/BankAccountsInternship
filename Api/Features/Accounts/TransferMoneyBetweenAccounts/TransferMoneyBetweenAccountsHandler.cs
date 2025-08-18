using Api.Abstractions;
using Api.Data;
using Api.Exceptions;
using Api.Exceptions.Extensions;
using Api.Features.Transactions.CreateTransaction;
using Api.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Text.Json;
using Api.Presentation.EventMessages;
using Api.Presentation.MessageEvents;

namespace Api.Features.Accounts.TransferMoneyBetweenAccounts;

public class TransferMoneyBetweenAccountsHandler(IAppDbContext context, IMediator mediator)
    : ICommandHandler<TransferMoneyBetweenAccountsCommand, Unit>
{
    public async Task<Unit> Handle(TransferMoneyBetweenAccountsCommand request, CancellationToken cancellationToken)
    {
        var dbContext = (AppDbContext) context;
        var conn = dbContext.Database.GetDbConnection();
        if (conn.State != ConnectionState.Open)
            await conn.OpenAsync(cancellationToken);

        await using var transaction = await conn.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        await dbContext.Database.UseTransactionAsync(transaction, cancellationToken);

        try
        {
            var senderAccount = dbContext.Accounts.FirstOrDefault(a => a.Id == request.SenderAccountId);
            var recipientAccount = dbContext.Accounts.FirstOrDefault(a => a.Id == request.RecipientAccountId);

            if (senderAccount == null)
                throw new NotFoundException(request.SenderAccountId.ToString());
            if (recipientAccount == null)
                throw new NotFoundException(request.RecipientAccountId.ToString());
            if (request.Amount > senderAccount.Balance)
                throw new BadRequestException("Amount more than available balance");
            if (senderAccount.Currency != recipientAccount.Currency)
                throw new BadRequestException("Incompatibility of currencies");
            if (DateTime.UtcNow > recipientAccount.ClosedDate)
                throw new BadRequestException("Cannot make transfer to closed account");

            var expectedSenderBalance = senderAccount.Balance - request.Amount;
            var expectedRecipientBalance = recipientAccount.Balance + request.Amount;

            await mediator.Send(new CreateTransactionCommand
            {
                AccountId = senderAccount.Id,
                CounterPartyAccountId = recipientAccount.Id,
                Amount = request.Amount,
                Currency = senderAccount.Currency,
                Type = TransactionType.Credit,
                Description = $"Transfer for {recipientAccount.Id}"
            }, cancellationToken);

            await mediator.Send(new CreateTransactionCommand
            {
                AccountId = recipientAccount.Id,
                CounterPartyAccountId = senderAccount.Id,
                Amount = request.Amount,
                Currency = recipientAccount.Currency,
                Type = TransactionType.Debit,
                Description = $"Transfer from {senderAccount.Id}"
            }, cancellationToken);

            context.Accounts.UpdateRange(senderAccount, recipientAccount);
            await context.SaveChangesAsync(cancellationToken);

            var occuredAt = DateTime.UtcNow;
            var outbox = new Outbox
            {
                Id = Guid.NewGuid(),
                OccurredAt = occuredAt,
                Type = "TransferCompleted",
                RoutingKey = "money.transfer.completed",
                Payload = JsonSerializer.Serialize(new EventMessage<TransferCompleted>
                {
                    EventId = Guid.NewGuid(),
                    OccurredAt = occuredAt,
                    Meta = new Meta(),
                    Payload = new TransferCompleted()
                    {
                        Amount = request.Amount,
                        Currency = senderAccount.Currency,
                        DestinationAccountId = recipientAccount.Id,
                        SourceAccountId = senderAccount.Id,
                        TransferId = Guid.NewGuid()
                    }
                })
            };

            context.Outbox.Add(outbox);

            if (senderAccount.Balance != expectedSenderBalance || recipientAccount.Balance != expectedRecipientBalance)
                throw new BadRequestException($"""
                                               Expected sender balance: {expectedSenderBalance}, current: {senderAccount.Balance}.
                                               Expected recipient balance: {expectedRecipientBalance}, current: {recipientAccount.Balance}.
                                               """);
            await context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return Unit.Value;
        }
        catch (InvalidOperationException ie) when (ExceptionExtensions.TryFindPostgresException(ie, out var pg) && pg!.SqlState == "40001")
        {
            await transaction.RollbackAsync(cancellationToken);
            throw new SerializationConflictException("Serialization conflict");
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}