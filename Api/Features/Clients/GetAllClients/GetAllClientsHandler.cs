using Api.Abstractions;
using Api.Data;
using Api.Features.Accounts.GetAllAccounts;
using Api.Features.Clients.GetAllClients;
using Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Api.Features.Accounts.GetAllClients
{
    public class GetAllClientsHandler : IQueryHandler<GetAllClientsQuery, IEnumerable<Client>>
    {
        private readonly IAppDbContext _context;

        public GetAllClientsHandler(IAppDbContext context)
        {
            _context = context;
        }
        
        public async Task<IEnumerable<Client>> Handle(GetAllClientsQuery request, CancellationToken cancellationToken)
        {
            var clients = await _context.Clients
                .Include(c => c.Accounts)
                .ThenInclude(a => a.Transactions)
                .ToListAsync(cancellationToken);

            return clients;
        }
    }
}
