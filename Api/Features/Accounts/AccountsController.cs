using Microsoft.AspNetCore.Mvc;
using Api.Exceptions;
using Api.Features.Accounts.AccountExists;
using Api.Features.Accounts.CreateAccount;
using Api.Features.Accounts.DeleteAccount;
using Api.Features.Accounts.GetAccount;
using Api.Features.Accounts.GetAllAccounts;
using Api.Features.Accounts.TransferMoneyBetweenAccounts;
using Api.Features.Accounts.UpdateAccount;
using Api.Features.Transactions;
using Api.Features.Transactions.GetTransaction;
using MediatR;

namespace Api.Features.Accounts
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class AccountsController(IMediator mediator) : ControllerBase
    {
        private readonly IMediator _mediator = mediator;

        #region CREATE
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType<TransactionDto>(StatusCodes.Status201Created)]
        [ProducesResponseType<ErrorRfc9910Dto>(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<AccountDto>> CreateAccount([FromBody] CreateAccountDto dto)
        {
            try
            {
                var account = await _mediator.Send(new CreateAccountCommand
                {
                    OwnerId = dto.OwnerId,
                    Type = dto.Type,
                    Currency = dto.Currency,
                    Balance = dto.Balance,
                    InterestRate = dto.InterestRate,
                    ClosedDate = dto.ClosedDate
                });
                return Created(".", account);
            }
            catch (NotFoundException e)
            {
                return NotFound(e.Message);
            }
        }

        [HttpPost("{accountId}/transfer")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType<ErrorRfc9910Dto>(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Transfer(Guid accountId, [FromBody] MoneyTransferDto dto)
        {
            await _mediator.Send(new TransferMoneyBetweenAccountsCommand
            {
                Amount = dto.Amount,
                RecipientAccountId = dto.RecipientId,
                SenderAccountId = accountId
            });

            return Created();
        }
        #endregion

        #region READ
        [HttpGet]
        [ProducesResponseType<IEnumerable<AccountDto>>(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<AccountDto>>> GetAccounts()
        {
            var accounts = await _mediator.Send(new GetAllAccountsQuery());
            return Ok(accounts);
        }
        
        [HttpGet("{accountId}")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType<TransactionDto>(StatusCodes.Status200OK)]
        [ProducesResponseType<ErrorRfc9910Dto>(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<AccountDto>> GetAccountById(Guid accountId)
        {
            var account = await _mediator.Send(new GetAccountQuery()
            {
                Id = accountId
            });
            
            return Ok(account);
        }

        [HttpGet("{accountId}/statement")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType<ErrorRfc9910Dto>(StatusCodes.Status404NotFound)]
        [ProducesResponseType<IEnumerable<TransactionDto>>(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<TransactionDto>>> GetStatement(Guid accountId, [FromQuery] GetStatementDto dto)
        {
            var transactions = await _mediator.Send(new GetStatementQuery()
            {
                AccountId = accountId,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate
            });
            return Ok(transactions);
        }

        // READ
        [HttpGet("exists/{accountId}/{ownerId}")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType<StatusDto>(StatusCodes.Status200OK)]
        public async Task<ActionResult<StatusDto>> ClientHasAccount(Guid accountId, Guid ownerId)
        {
            var status = await _mediator.Send(new AccountExistsQuery()
            {
                Id = accountId,
                OwnerId = ownerId
            });
            return Ok(new StatusDto { Status = status });
        }
        #endregion

        #region UPDATE
        [HttpPatch("{accountId}")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status304NotModified)]
        [ProducesResponseType<AccountDto>(StatusCodes.Status200OK)]
        [ProducesResponseType<ErrorRfc9910Dto>(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<AccountDto>> UpdateAccount(Guid accountId, [FromBody] UpdateAccountDto dto)
        {
            var account = await _mediator.Send(new UpdateAccountCommand()
            {
                Id = accountId,
                InterestRate = dto.InterestRate,
                ClosedDate = dto.ClosedDate
            });
            
            if (account == null) return StatusCode(StatusCodes.Status304NotModified);
            
            return Ok(account);
        }
        #endregion

        #region DELETE
        [HttpDelete("{accountId}")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType<ErrorRfc9910Dto>(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteAccount(Guid accountId)
        {
            await _mediator.Send(new DeleteAccountCommand() { Id = accountId });

            return NoContent();
        }
        #endregion
    }
}
