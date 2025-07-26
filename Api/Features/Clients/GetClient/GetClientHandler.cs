using Api.Abstractions;
using Api.Data;
using Api.Exceptions;
using Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Api.Features.Clients.GetClient;

public class GetClientHandler(IAppDbContext context) : IQueryHandler<GetClientQuery, Client>
{
    public async Task<Client> Handle(GetClientQuery request, CancellationToken cancellationToken)
    {
        var client = await context.Clients.FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);
        if (client == null) throw new NotFoundException();

        return client;
    }
}