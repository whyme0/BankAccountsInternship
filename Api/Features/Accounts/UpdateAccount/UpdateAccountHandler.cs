using Api.Abstractions;
using Api.Data;
using Api.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Api.Features.Accounts.UpdateAccount
{
    public class UpdateAccountHandler : ICommandHandler<UpdateAccountCommand, AccountDto>
    {
        private readonly IAppDbContext _context;

        public UpdateAccountHandler(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<AccountDto> Handle(UpdateAccountCommand request, CancellationToken cancellationToken)
        {
            var account = await _context.Accounts
                .Include(a => a.Owner)
                .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);
            if (account == null) throw new NotFoundException(request.Id.ToString());

            if (request.InterestRate != null)
            {
                account.InterestRate = (decimal) request.InterestRate;
            }
            if (request.ClosedDate != null)
            {
                account.ClosedDate = request.ClosedDate;
            }

            _context.Accounts.Update(account);
            await _context.SaveChangesAsync(cancellationToken);

            return new AccountDto()
            {
                Balance = account.Balance,
                ClosedDate = account.ClosedDate,
                Currency = account.Currency,
                Id = account.Id,
                InterestRate = account.InterestRate,
                OpenedDate = account.OpenedDate,
                Owner = new ClientDto()
                {
                    Id = account.Owner.Id,
                    Name = account.Owner.Name
                },
                Type = account.Type,
                Transactions = new List<TransactionDto>()
            };
        }
    }
}
