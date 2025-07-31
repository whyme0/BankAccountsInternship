using Api.Features.Accounts;
using Api.Features.Transactions.CreateTransaction;
using Api.Presentation;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Api.Features.Transactions;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class TransactionsController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<TransactionDto>(StatusCodes.Status201Created)]
    public async Task<MbResult<TransactionDto>> CreateTransaction([FromBody] CreateTransactionDto dto)
    {
        var transaction = await mediator.Send(new CreateTransactionCommand
        {
            AccountId = dto.AccountId,
            Amount = dto.Amount,
            CounterPartyAccountId = dto.CounterPartyAccountId,
            Currency = dto.Currency,
            Description = dto.Description,
            Type = dto.Type
        });

        return new MbResult<TransactionDto>
        { 
            Value = new TransactionDto
            {
                AccountId = transaction.AccountId,
                Amount = transaction.Amount,
                CounterPartyAccountId = transaction.CounterPartyAccountId,
                Currency = transaction.Currency,
                Date = transaction.Date,
                Description = transaction.Description,
                Id = transaction.Id,
                Type = transaction.Type
            },
            StatusCode = StatusCodes.Status201Created
        };
    }
}