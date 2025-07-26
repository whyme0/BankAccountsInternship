using Api.Abstractions;
using Api.Data;
using Microsoft.EntityFrameworkCore;

namespace Api.Features.Accounts.AccountExists
{
    public class AccountExistsHandler : IQueryHandler<AccountExistsQuery, bool>
    {
        private readonly IAppDbContext _context;
        
        public AccountExistsHandler(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<bool> Handle(AccountExistsQuery request, CancellationToken cancellationToken)
        {
            return await _context.Accounts.FirstOrDefaultAsync(a => a.Id == request.Id && a.OwnerId == request.OwnerId, cancellationToken) !=
                   null;
        }
    }
}