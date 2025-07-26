using Api.Abstractions;
using Api.Data;
using Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Api.Features.Clients.GetAllClients;

public class GetAllClientsHandler(IAppDbContext context) : IQueryHandler<GetAllClientsQuery, IEnumerable<Client>>
{
    public async Task<IEnumerable<Client>> Handle(GetAllClientsQuery request, CancellationToken cancellationToken)
    {
        var clients = await context.Clients
            .Include(c => c.Accounts)
            .ThenInclude(a => a.Transactions)
            .ToListAsync(cancellationToken);

        return clients;
    }
}