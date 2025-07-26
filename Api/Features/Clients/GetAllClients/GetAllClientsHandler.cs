using Api.Abstractions;
using Api.Data;
using Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Api.Features.Clients.GetAllClients
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
