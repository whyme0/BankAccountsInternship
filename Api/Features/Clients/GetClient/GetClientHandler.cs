using Api.Abstractions;
using Api.Data;
using Api.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Api.Features.Clients.GetClient;

public class GetClientHandler(IAppDbContext context) : IQueryHandler<GetClientQuery, ClientDto>
{
    public async Task<ClientDto> Handle(GetClientQuery request, CancellationToken cancellationToken)
    {
        var client = await context.Clients.FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);
        if (client == null) throw new NotFoundException();

        return new ClientDto
        {
            Id = client.Id,
            Name = client.Name
        };
    }
}