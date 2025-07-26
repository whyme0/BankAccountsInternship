using Api.Features.Clients.GetAllClients;
using Api.Features.Clients.GetClient;
using Api.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Api.Features.Clients;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ClientsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Client>>> GetClients()
    {
        var clients = await mediator.Send(new GetAllClientsQuery());
            
        return Ok(clients);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<Client>> GetClientById(Guid id)
    {
        var client = await mediator.Send(new GetClientQuery { Id = id });

        return Ok(client);
    }
}