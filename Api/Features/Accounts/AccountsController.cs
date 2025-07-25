using Api.Models;
using Microsoft.AspNetCore.Mvc;
using Api.Data;
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
using Microsoft.EntityFrameworkCore;

namespace Api.Features.Accounts
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class AccountsController : ControllerBase
    {
        private readonly IAppDbContext _context;
        private readonly IMediator _mediator;

        public AccountsController(IAppDbContext context, IMediator mediator)
        {
            _context = context;
            _mediator = mediator;
        }

        #region CREATE
        [HttpPost]
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

        [HttpPost("transfer")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType<ErrorRfc9910Dto>(StatusCodes.Status404NotFound)]
        [ProducesResponseType<ErrorRfc9910Dto>(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Transfer([FromBody] MoneyTransferDto dto)
        {
            try
            {
                await _mediator.Send(new TransferMoneyBetweenAccountsCommand
                {
                    Amount = dto.Amount,
                    RecipientAccountId = dto.RecipientId,
                    SenderAccountId = dto.SenderId
                });
            }
            catch (NotFoundException e)
            {
                return NotFound(new { e.Message });
            }
            catch (Exception e)
            {
                return BadRequest(new { e.Message });
            }

            return Ok();
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
        [ProducesResponseType<TransactionDto>(StatusCodes.Status200OK)]
        [ProducesResponseType<ErrorRfc9910Dto>(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<AccountDto>> GetAccountById(Guid accountId)
        {
            try
            {
                var account = await _mediator.Send(new GetAccountQuery()
                {
                    Id = accountId
                });
                
                return Ok(account);
            }
            catch (NotFoundException e)
            {
                return NotFound(e.Message);
            }
        }

        [HttpGet("{accountId}/statement")]
        [ProducesResponseType<IEnumerable<TransactionDto>>(StatusCodes.Status200OK)]
        [ProducesResponseType<ErrorRfc9910Dto>(StatusCodes.Status404NotFound)]
        [ProducesResponseType<ErrorRfc9910Dto>(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IEnumerable<TransactionDto>>> GetStatement(Guid accountId, [FromBody] GetStatementDto dto)
        {
            try
            {
                var transactions = await _mediator.Send(new GetStatementQuery()
                {
                    AccountId = accountId,
                    StartDate = dto.StartDate,
                    EndDate = dto.EndDate
                });
                return Ok(transactions);
            }
            catch (NotFoundException)
            {
                return NotFound();
            }
            catch
            {
                return BadRequest();
            }
        }

        // READ
        [HttpGet("exists/{accountId}/{ownerId}")]
        [ProducesResponseType<StatusDto>(StatusCodes.Status200OK)]
        public async Task<ActionResult<StatusDto>> ClientHasAccount(Guid accountId, Guid ownerId)
        {
            var status = await _mediator.Send(new AccountExitstsQuery()
            {
                Id = accountId,
                OwnerId = ownerId
            });
            return Ok(new StatusDto { Status = status });
        }
        #endregion

        #region UPDATE
        [HttpPatch("{accountId}")]
        [ProducesResponseType<AccountDto>(StatusCodes.Status200OK)]
        [ProducesResponseType<ErrorRfc9910Dto>(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<AccountDto>> UpdateAccount(Guid accountId, [FromBody] UpdateAccountDto dto)
        {
            try
            {
                var account = await _mediator.Send(new UpdateAccountCommand()
                {
                    Id = accountId,
                    InterestRate = dto.InterestRate,
                    ClosedDate = dto.ClosedDate
                });
                return Ok(account);
            }
            catch (NotFoundException e)
            {
                return NotFound(new { e.Message });
            }
        }
        #endregion

        #region DELETE
        [HttpDelete("{accountId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType<ErrorRfc9910Dto>(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteAccount(Guid accountId)
        {
            try
            {
                await _mediator.Send(new DeleteAccountCommand() { Id = accountId });
            }
            catch (NotFoundException e)
            {
                return NotFound(e.Message);
            }

            return NoContent();
        }
        #endregion

        // Эндпоинт для отладки. В api сценариях технического задания не учавствует
        [HttpGet("clients")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<ActionResult<IEnumerable<Client>>> GetClients()
        {
            return await _context.Clients
                .Include(c => c.Accounts)
                .ThenInclude(a => a.Transactions)
                .ToListAsync();
        }
    }
}
