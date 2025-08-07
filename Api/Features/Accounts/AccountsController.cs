using Microsoft.AspNetCore.Mvc;
using Api.Features.Accounts.AccountExists;
using Api.Features.Accounts.CreateAccount;
using Api.Features.Accounts.DeleteAccount;
using Api.Features.Accounts.GetAccount;
using Api.Features.Accounts.GetAllAccounts;
using Api.Features.Accounts.TransferMoneyBetweenAccounts;
using Api.Features.Accounts.UpdateAccount;
using Api.Features.Transactions;
using Api.Features.Transactions.GetStatement;
using MediatR;
using Api.Presentation;

namespace Api.Features.Accounts;

/// <summary>
/// Раздел отвечающий за операции со счетами
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[ProducesResponseType(StatusCodes.Status500InternalServerError)]
[ProducesResponseType<MbResult>(StatusCodes.Status401Unauthorized)]
public class AccountsController(IMediator mediator) : ControllerBase
{
    #region CREATE
    /// <summary>
    /// Открывает новый счет клиенту
    /// </summary>
    /// <param name="dto">Тело запроса с параметрами для открытия счета</param>
    /// <returns>MbResult&lt;T&gt;</returns>
    [HttpPost]
    [ProducesResponseType<MbResult>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<MbResult>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<MbResult<AccountDto>>(StatusCodes.Status201Created)]
    public async Task<MbResult<AccountDto>> CreateAccount([FromBody] CreateAccountDto dto)
    {
        var account = await mediator.Send(new CreateAccountCommand
        {
            OwnerId = dto.OwnerId,
            Type = dto.Type,
            Currency = dto.Currency,
            Balance = dto.Balance,
            InterestRate = dto.InterestRate,
            ClosedDate = dto.ClosedDate
        });
        return new MbResult<AccountDto> {Value = account, StatusCode = StatusCodes.Status201Created};
    }

    /// <summary>
    /// Осуществляет перевод между двумя указанными счетами
    /// </summary>
    /// <param name="accountId">Идентификатор счета, который предствляет отправителя денежных средств</param>
    /// <param name="dto">Тело запроса для осуществление перевода</param>
    /// <returns>MbResult&lt;T&gt;</returns>
    [HttpPost("{accountId:guid}/transfer")]
    [ProducesResponseType<MbResult>(StatusCodes.Status201Created)]
    [ProducesResponseType<MbResult>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<MbResult>(StatusCodes.Status400BadRequest)]
    public async Task<MbResult> Transfer(Guid accountId, [FromBody] MoneyTransferDto dto)
    {
        await mediator.Send(new TransferMoneyBetweenAccountsCommand
        {
            Amount = dto.Amount,
            RecipientAccountId = dto.RecipientId,
            SenderAccountId = accountId
        });

        return new MbResult {StatusCode = StatusCodes.Status201Created};
    }
    #endregion

    #region READ
    /// <summary>
    /// Метод для получения списка всех счетов в базе данных
    /// </summary>
    /// <returns>MbResult&lt;T&gt;</returns>
    [HttpGet]
    [ProducesResponseType<MbResult<IEnumerable<AccountDto>>>(StatusCodes.Status200OK)]
    public async Task<MbResult<IEnumerable<AccountDto>>> GetAccounts()
    {
        var accounts = await mediator.Send(new GetAllAccountsQuery());
        return new MbResult<IEnumerable<AccountDto>> { Value = accounts, StatusCode = StatusCodes.Status200OK };
    }


    /// <summary>
    /// Получает по счет по соответствующему guid идентификатору
    /// </summary>
    /// <param name="accountId">Уникальный индентификатор счета</param>
    /// <returns>MbResult&lt;T&gt;</returns>
    [HttpGet("{accountId:guid}")]
    [ProducesResponseType<MbResult>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<MbResult>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<MbResult<TransactionDto>>(StatusCodes.Status200OK)]
    public async Task<MbResult<AccountDto>> GetAccountById(Guid accountId)
    {
        var account = await mediator.Send(new GetAccountQuery
        {
            Id = accountId
        });
            
        return new MbResult<AccountDto> {Value=account, StatusCode = StatusCodes.Status200OK};
    }

    /// <summary>
    /// Получает выписку за указанный промежуток времени
    /// </summary>
    /// <param name="accountId">Идентификатор счета для которого осуществляется выписка</param>
    /// <param name="dto">Тело запроса, в котором указывается интервалы</param>
    /// <returns>MbResult&lt;T&gt;</returns>
    [HttpGet("{accountId:guid}/statement")]
    [ProducesResponseType<MbResult>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<MbResult>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<MbResult<IEnumerable<TransactionDto>>>(StatusCodes.Status200OK)]
    public async Task<MbResult<IEnumerable<TransactionDto>>> GetStatement(Guid accountId, [FromQuery] GetStatementDto dto)
    {
        var transactions = await mediator.Send(new GetStatementQuery
        {
            AccountId = accountId,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate
        });
        return new MbResult<IEnumerable<TransactionDto>> { Value = transactions, StatusCode = StatusCodes.Status200OK };
    }

    /// <summary>
    /// Проверить существование определенного счета у определенного клиента
    /// </summary>
    /// <param name="accountId">Идентификатор счета</param>
    /// <param name="ownerId">Идентификатор клиента</param>
    /// <returns>MbResult&lt;T&gt;</returns>
    [HttpGet("exists/{accountId:guid}/{ownerId:guid}")]
    [ProducesResponseType<MbResult>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<MbResult<StatusDto>>(StatusCodes.Status200OK)]
    public async Task<MbResult<StatusDto>> ClientHasAccount(Guid accountId, Guid ownerId)
    {
        var status = await mediator.Send(new AccountExistsQuery
        {
            Id = accountId,
            OwnerId = ownerId
        });
        return new MbResult<StatusDto>
        {
            Value = new StatusDto { Status = status },
            StatusCode = StatusCodes.Status200OK
        };
    }
    #endregion

    #region UPDATE
    /// <summary>
    /// Изменение полей счета
    /// </summary>
    /// <remarks>Можно изменить только процентную ставку и дату закрытия счета</remarks>
    /// <param name="accountId">Уникальный идентификатор счета</param>
    /// <param name="dto">Тело, которое передается в запрос</param>
    /// <returns>MbResult&lt;T&gt;</returns>
    [HttpPatch("{accountId:guid}")]
    [ProducesResponseType<MbResult>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<MbResult>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<MbResult>(StatusCodes.Status304NotModified)]
    [ProducesResponseType<MbResult<AccountDto>>(StatusCodes.Status200OK)]
    public async Task<MbResult<AccountDto>> UpdateAccount(Guid accountId, [FromBody] UpdateAccountDto dto)
    {
        var account = await mediator.Send(new UpdateAccountCommand
        {
            Id = accountId,
            InterestRate = dto.InterestRate,
            ClosedDate = dto.ClosedDate
        });
            
        if (account == null) return new MbResult<AccountDto> { StatusCode = StatusCodes.Status304NotModified };
            
        return new MbResult<AccountDto>
        {
            Value = account,
            StatusCode = StatusCodes.Status200OK
        };
    }
    #endregion

    #region DELETE
    /// <summary>
    /// Удалить счет по указанному идентификатору счета
    /// </summary>
    /// <param name="accountId">Уникальный идентификатор счета</param>
    /// <returns>MbResult&lt;T&gt;</returns>
    [HttpDelete("{accountId:guid}")]
    [ProducesResponseType<MbResult>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<MbResult>(StatusCodes.Status204NoContent)]
    [ProducesResponseType<MbResult>(StatusCodes.Status400BadRequest)]
    public async Task<MbResult> DeleteAccount(Guid accountId)
    {
        await mediator.Send(new DeleteAccountCommand { Id = accountId });

        return new MbResult {StatusCode = StatusCodes.Status204NoContent};
    }
    #endregion
}