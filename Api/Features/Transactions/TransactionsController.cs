using Api.Features.Accounts;
using Api.Features.Transactions.CreateTransaction;
using Api.Presentation;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Api.Features.Transactions;

/// <summary>
/// Раздел отвечающий за операции с транзакциями
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[ProducesResponseType(StatusCodes.Status500InternalServerError)]
[ProducesResponseType<MbResult>(StatusCodes.Status401Unauthorized)]
public class TransactionsController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Создает транзакцию
    /// </summary>
    /// <remarks>Рекомендуется использовать в случаях зачисления/снятия денежных средств с банкомата, когда отсутствует контрагент. В остальных случаях стоит использовать `Transfer` метод контроллера `AccountsController`</remarks>
    /// <param name="dto">Тело запроса для создания модели</param>
    /// <returns>MbResult&lt;T&gt;</returns>
    [HttpPost]
    [ProducesResponseType<MbResult>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<MbResult>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<MbResult<TransactionDto>>(StatusCodes.Status201Created)]
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