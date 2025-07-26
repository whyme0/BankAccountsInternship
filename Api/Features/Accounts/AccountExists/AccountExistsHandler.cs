using Api.Abstractions;
using Api.Data;
using Microsoft.EntityFrameworkCore;

namespace Api.Features.Accounts.AccountExists;

public class AccountExistsHandler(IAppDbContext context) : IQueryHandler<AccountExistsQuery, bool>
{
    public async Task<bool> Handle(AccountExistsQuery request, CancellationToken cancellationToken)
    {
        return await context.Accounts.FirstOrDefaultAsync(a => a.Id == request.Id && a.OwnerId == request.OwnerId, cancellationToken) !=
               null;
    }
}