using Api.Abstractions;
using Api.Data;
using Api.Exceptions;
using Api.Features.Transactions.CreateTransaction;
using Api.Models;
using MediatR;

namespace Api.Features.Accounts.TransferMoneyBetweenAccounts
{
    public class TransferMoneyBetweenAccountsHandler : ICommandHandler<TransferMoneyBetweenAccountsCommand>
    {
        private readonly IAppDbContext _context;
        private readonly IMediator _mediator;
        
        public TransferMoneyBetweenAccountsHandler(IAppDbContext context, IMediator mediator)
        {
            _context = context;
            _mediator = mediator;
        }

        public async Task Handle(TransferMoneyBetweenAccountsCommand request, CancellationToken cancellationToken)
        {
            var senderAccount = _context.Accounts.FirstOrDefault(a => a.Id == request.SenderAccountId);
            var recipientAccount = _context.Accounts.FirstOrDefault(a => a.Id == request.RecipientAccountId);

            if (senderAccount == null) throw new NotFoundException(request.SenderAccountId.ToString());
            if (recipientAccount == null) throw new NotFoundException(request.RecipientAccountId.ToString());
            if (request.Amount > senderAccount.Balance) throw new BadRequestException("Amount more than available balance");
            if (senderAccount.Currency != recipientAccount.Currency) throw new BadRequestException("Incompatibility of currencies");
            if (DateTime.UtcNow > recipientAccount.ClosedDate)
                throw new BadRequestException("Cannot make transfer to closed account");

            var currentDate = DateTime.UtcNow;

            await _mediator.Send(new CreateTransactionCommand()
            {
                AccountId = senderAccount.Id,
                CounterPartyAccountId = recipientAccount.Id,
                Amount = -request.Amount,
                Currency = senderAccount.Currency,
                Type = TransactionType.Credit,
                Description = $"Transfer for {recipientAccount.Id}",
                Date = currentDate
            }, cancellationToken);

            await _mediator.Send(new CreateTransactionCommand
            {
                AccountId = recipientAccount.Id,
                CounterPartyAccountId = senderAccount.Id,
                Amount = request.Amount,
                Currency = recipientAccount.Currency,
                Type = TransactionType.Debit,
                Description = $"Transfer from {senderAccount.Id}",
                Date = currentDate
            }, cancellationToken);


            _context.Accounts.UpdateRange(senderAccount, recipientAccount);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
