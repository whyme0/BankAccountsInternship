using Api.Abstractions;
using Api.Models;

namespace Api.Features.Clients.GetClient
{
    public class GetClientQuery : IQuery<Client>
    {
        public Guid Id { get; set; }
    }
}
