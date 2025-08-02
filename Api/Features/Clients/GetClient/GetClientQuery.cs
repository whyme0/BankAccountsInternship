using Api.Abstractions;

// ReSharper disable PropertyCanBeMadeInitOnly.Global

namespace Api.Features.Clients.GetClient;

public class GetClientQuery : IQuery<ClientDto>
{
    public Guid Id { get; set; }
}