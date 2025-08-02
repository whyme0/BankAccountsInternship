using Api.Features.Clients.GetAllClients;
using Api.Features.Clients.GetClient;
using Api.Models;
using Api.Presentation;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Api.Features.Clients;

/// <summary>
/// Раздел отвечающий за операции с клиентами
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[ProducesResponseType(StatusCodes.Status500InternalServerError)]
public class ClientsController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Получает список всех существующих клиентов в системе
    /// </summary>
    /// <returns>MbResult&lt;T&gt;</returns>
    [HttpGet]
    [ProducesResponseType<MbResult<IEnumerable<Client>>>(StatusCodes.Status200OK)]
    public async Task<MbResult<IEnumerable<Client>>> GetClients()
    {
        var clients = await mediator.Send(new GetAllClientsQuery());
            
        return new MbResult<IEnumerable<Client>>
        {
            Value = clients,
            StatusCode = StatusCodes.Status200OK
        };
    }

    /// <summary>
    /// Получить клиента по заданному идентификатору типа Guid
    /// </summary>
    /// <param name="id">Уникальный идентификатор пользователя</param>
    /// <returns>MbResult&lt;T&gt;</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<MbResult>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<MbResult>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<MbResult<ClientDto>>(StatusCodes.Status200OK)]
    public async Task<MbResult<ClientDto>> GetClientById(Guid id)
    {
        var client = await mediator.Send(new GetClientQuery { Id = id });

        return new MbResult<ClientDto>
        {
            Value = client,
            StatusCode = StatusCodes.Status200OK
        };
    }
}