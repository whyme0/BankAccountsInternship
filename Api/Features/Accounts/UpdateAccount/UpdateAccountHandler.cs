using Api.Abstractions;
using Api.Data;
using Api.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Api.Features.Accounts.UpdateAccount;

public class UpdateAccountHandler(IAppDbContext context) : ICommandHandler<UpdateAccountCommand, AccountDto?>
{
    public async Task<AccountDto?> Handle(UpdateAccountCommand request, CancellationToken cancellationToken)
    {
        var account = await context.Accounts
            .Include(a => a.Owner)
            .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);
        if (account == null) throw new NotFoundException();

        var isUnchanged =
            (!request.InterestRate.HasValue || request.InterestRate == account.InterestRate)
            && (!request.ClosedDate.HasValue || request.ClosedDate == account.ClosedDate);
        if (isUnchanged) return null;

        if (request.InterestRate != null)
        {
            account.InterestRate = (decimal) request.InterestRate;
        }
        if (request.ClosedDate != null)
        {
            account.ClosedDate = request.ClosedDate;
        }

        context.Accounts.Update(account);
        await context.SaveChangesAsync(cancellationToken);

        return new AccountDto
        {
            Balance = account.Balance,
            ClosedDate = account.ClosedDate,
            Currency = account.Currency,
            Id = account.Id,
            InterestRate = account.InterestRate,
            OpenedDate = account.OpenedDate,
            Owner = new ClientDto
            {
                Id = account.Owner.Id,
                Name = account.Owner.Name
            },
            Type = account.Type,
            Transactions = new List<TransactionDto>()
        };
    }
}