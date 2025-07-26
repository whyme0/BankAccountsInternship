using Api.Abstractions;
using Api.Data;
using Api.Exceptions;
using Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Api.Features.Clients.GetClient
{
    public class GetClientHandler : IQueryHandler<GetClientQuery, Client>
    {
        private readonly IAppDbContext _context;
        public GetClientHandler(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<Client> Handle(GetClientQuery request, CancellationToken cancellationToken)
        {
            var client = await _context.Clients.FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);
            if (client == null) throw new NotFoundException();

            return client;
        }
    }
}
