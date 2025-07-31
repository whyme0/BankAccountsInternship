using Api.Features.Clients.GetAllClients;
using Api.Features.Clients.GetClient;
using Api.Models;
using Api.Presentation;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Api.Features.Clients;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ClientsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IEnumerable<Client>>(StatusCodes.Status200OK)]
    public async Task<MbResult<IEnumerable<Client>>> GetClients()
    {
        var clients = await mediator.Send(new GetAllClientsQuery());
            
        return new MbResult<IEnumerable<Client>>
        {
            Value = clients,
            StatusCode = StatusCodes.Status200OK
        };
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<IEnumerable<Client>>(StatusCodes.Status200OK)]
    public async Task<MbResult<Client>> GetClientById(Guid id)
    {
        var client = await mediator.Send(new GetClientQuery { Id = id });

        return new MbResult<Client>
        {
            Value = client,
            StatusCode = StatusCodes.Status200OK
        };
    }
}