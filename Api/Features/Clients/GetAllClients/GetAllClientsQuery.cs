using Api.Abstractions;
using Api.Models;

namespace Api.Features.Clients.GetAllClients;

public class GetAllClientsQuery : IQuery<IEnumerable<Client>>;