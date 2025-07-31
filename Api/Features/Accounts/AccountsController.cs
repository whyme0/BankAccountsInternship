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

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AccountsController(IMediator mediator) : ControllerBase
{

    #region CREATE
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<TransactionDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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

    [HttpPost("{accountId:guid}/transfer")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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
    [HttpGet]
    [ProducesResponseType<IEnumerable<AccountDto>>(StatusCodes.Status200OK)]
    public async Task<MbResult<IEnumerable<AccountDto>>> GetAccounts()
    {
        var accounts = await mediator.Send(new GetAllAccountsQuery());
        return new MbResult<IEnumerable<AccountDto>> { Value = accounts, StatusCode = StatusCodes.Status200OK };
    }

    [HttpGet("{accountId:guid}")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<TransactionDto>(StatusCodes.Status200OK)]
    public async Task<MbResult<AccountDto>> GetAccountById(Guid accountId)
    {
        var account = await mediator.Send(new GetAccountQuery
        {
            Id = accountId
        });
            
        return new MbResult<AccountDto> {Value=account, StatusCode = StatusCodes.Status200OK};
    }

    [HttpGet("{accountId:guid}/statement")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<IEnumerable<TransactionDto>>(StatusCodes.Status200OK)]
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

    // READ
    [HttpGet("exists/{accountId:guid}/{ownerId:guid}")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<StatusDto>(StatusCodes.Status200OK)]
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
    [HttpPatch("{accountId:guid}")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status304NotModified)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType<AccountDto>(StatusCodes.Status200OK)]
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
    [HttpDelete("{accountId:guid}")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<MbResult> DeleteAccount(Guid accountId)
    {
        await mediator.Send(new DeleteAccountCommand { Id = accountId });

        return new MbResult {StatusCode = StatusCodes.Status204NoContent};
    }
    #endregion
}