using System.Reflection.Metadata.Ecma335;
using Api.Data;
using Api.Features.Clients.GetAllClients;
using Api.Features.Clients.GetClient;
using Api.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Features.Clients
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class ClientsController(IAppDbContext context, IMediator mediator) : ControllerBase
    {
        private readonly IAppDbContext _context = context;
        private readonly IMediator _mediator = mediator;

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Client>>> GetClients()
        {
            var clients = await _mediator.Send(new GetAllClientsQuery());
            
            return Ok(clients);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Client>> GetClientById(Guid id)
        {
            var client = await _mediator.Send(new GetClientQuery() { Id = id });

            return Ok(client);
        }
    }
}