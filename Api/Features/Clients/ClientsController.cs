using Api.Features.Clients.BlockClient;
using Api.Features.Clients.GetAllClients;
using Api.Features.Clients.GetClient;
using Api.Features.Clients.UnblockClient;
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
[ProducesResponseType<MbResult>(StatusCodes.Status401Unauthorized)]
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

    /// <summary>
    /// Заблокировать клиенту возможность работать со счетами
    /// </summary>
    /// <param name="id">Уникальный идентификатор пользователя</param>
    /// <returns>MbResult</returns>
    [HttpPost("{id:guid}/block")]
    [ProducesResponseType<MbResult>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<MbResult>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<MbResult<ClientDto>>(StatusCodes.Status202Accepted)]
    public async Task<MbResult> BlockClient(Guid id)
    {
        await mediator.Send(new BlockClientCommand { Id = id });
        
        return new MbResult
        {
            StatusCode = StatusCodes.Status202Accepted
        };
    }

    /// <summary>
    /// Заблокировать клиенту возможность работать со счетами
    /// </summary>
    /// <param name="id">Уникальный идентификатор пользователя</param>
    /// <returns>MbResult</returns>
    [HttpPost("{id:guid}/unblock")]
    [ProducesResponseType<MbResult>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<MbResult>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<MbResult<ClientDto>>(StatusCodes.Status202Accepted)]
    public async Task<MbResult> UnblockClient(Guid id)
    {
        await mediator.Send(new UnblockClientCommand { Id = id });

        return new MbResult
        {
            StatusCode = StatusCodes.Status202Accepted
        };
    }
}