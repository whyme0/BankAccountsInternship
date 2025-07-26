using Api.Exceptions;
using Api.Features.Accounts;
using Api.Features.Transactions.CreateTransaction;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Api.Features.Transactions
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class TransactionsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public TransactionsController(IMediator mediator)
        {
            _mediator = mediator;
        }
        
        [HttpPost]
        [ProducesResponseType<TransactionDto>(StatusCodes.Status201Created)]
        [ProducesResponseType<ErrorRfc9910Dto>(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<TransactionDto>> CreateTransaction([FromBody] CreateTransactionDto dto)
        {
            var transaction = await _mediator.Send(new CreateTransactionCommand()
            {
                AccountId = dto.AccountId,
                Amount = dto.Amount,
                CounterPartyAccountId = dto.CounterPartyAccountId,
                Currency = dto.Currency,
                Date = DateTime.UtcNow,
                Description = dto.Description,
                Type = dto.Type
            });

            return Created(".", transaction);
        }
    }
}
