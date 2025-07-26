using Api.Abstractions;
using Api.Data;
using Api.Exceptions;
using Api.Features.Transactions.CreateTransaction;
using Api.Models;
using MediatR;

namespace Api.Features.Accounts.TransferMoneyBetweenAccounts;

public class TransferMoneyBetweenAccountsHandler(IAppDbContext context, IMediator mediator)
    : ICommandHandler<TransferMoneyBetweenAccountsCommand, Unit>
{
    public async Task<Unit> Handle(TransferMoneyBetweenAccountsCommand request, CancellationToken cancellationToken)
    {
        var senderAccount = context.Accounts.FirstOrDefault(a => a.Id == request.SenderAccountId);
        var recipientAccount = context.Accounts.FirstOrDefault(a => a.Id == request.RecipientAccountId);

        if (senderAccount == null) throw new NotFoundException(request.SenderAccountId.ToString());
        if (recipientAccount == null) throw new NotFoundException(request.RecipientAccountId.ToString());
        if (request.Amount > senderAccount.Balance) throw new BadRequestException("Amount more than available balance");
        if (senderAccount.Currency != recipientAccount.Currency) throw new BadRequestException("Incompatibility of currencies");
        if (DateTime.UtcNow > recipientAccount.ClosedDate) throw new BadRequestException("Cannot make transfer to closed account");

        await mediator.Send(new CreateTransactionCommand
        {
            AccountId = senderAccount.Id,
            CounterPartyAccountId = recipientAccount.Id,
            Amount = -request.Amount,
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

        return Unit.Value;
    }
}