using Api.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using Api.Data;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AccountsController(AppDbContext context)
        {
            _context = context;
        }

        // 1. Создать счёт
        [HttpPost]
        public async Task<ActionResult<Account>> CreateAccount([FromBody] Account account)
        {
            account.Id = Guid.NewGuid();
            account.OpenedDate = DateTime.UtcNow;
            _context.Accounts.Add(account);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetAccountById), new { accountId = account.Id }, account);
        }

        // 2. Изменить счёт
        [HttpPut("{accountId}")]
        public async Task<IActionResult> UpdateAccount(Guid accountId, [FromBody] Account updated)
        {
            if (accountId != updated.Id) return BadRequest("ID mismatch");
            var existing = await _context.Accounts.FindAsync(accountId);
            if (existing == null) return NotFound();

            existing.Type = updated.Type;
            existing.Currency = updated.Currency;
            existing.InterestRate = updated.InterestRate;
            existing.ClosedDate = updated.ClosedDate;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // 3. Удалить счёт
        [HttpDelete("{accountId}")]
        public async Task<IActionResult> DeleteAccount(Guid accountId)
        {
            var account = await _context.Accounts.FindAsync(accountId);
            if (account == null) return NotFound();

            _context.Accounts.Remove(account);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // 4. Получить список счетов
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Account>>> GetAccounts()
        {
            return await _context.Accounts
                .Include(a => a.Owner)
                .ToListAsync();
        }

        // Дополнительно: получить счёт по ID
        [HttpGet("{accountId}")]
        public async Task<ActionResult<Account>> GetAccountById(Guid accountId)
        {
            var account = await _context.Accounts
                .Include(a => a.Owner)
                .Include(a => a.Transactions)
                .FirstOrDefaultAsync(a => a.Id == accountId);

            if (account == null) return NotFound();
            return account;
        }

        // 5. Зарегистрировать транзакцию по счёту
        [HttpPost("{accountId}/transactions")]
        public async Task<IActionResult> RegisterTransaction(Guid accountId, [FromBody] Transaction transaction)
        {
            var account = await _context.Accounts.FindAsync(accountId);
            if (account == null) return NotFound("Account not found");

            transaction.Id = Guid.NewGuid();
            transaction.AccountId = accountId;
            transaction.Date = DateTime.UtcNow;

            account.Balance += transaction.Type == TransactionType.Credit
                ? transaction.Amount
                : -transaction.Amount;

            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();

            return Created($"api/accounts/{accountId}/transactions/{transaction.Id}", transaction);
        }

        // 6. Перевод между счетами
        [HttpPost("{fromId}/{toId}/transfer")]
        public async Task<IActionResult> Transfer(Guid fromId, Guid toId, decimal amount, string? description)
        {
            if (amount <= 0) return BadRequest("Amount must be greater than zero");

            var source = await _context.Accounts.FindAsync(fromId);
            var target = await _context.Accounts.FindAsync(toId);

            if (source == null || target == null) return NotFound("Source or target account not found");
            if (source.Balance < amount) return BadRequest("Insufficient funds");

            var now = DateTime.UtcNow;

            var debitTx = new Transaction
            {
                Id = Guid.NewGuid(),
                AccountId = source.Id,
                CounterPartyAccountId = target.Id,
                Amount = amount,
                Currency = source.Currency,
                Type = TransactionType.Debit,
                Date = now,
                Description = description ?? $"Transfer to {toId}"
            };

            var creditTx = new Transaction
            {
                Id = Guid.NewGuid(),
                AccountId = target.Id,
                CounterPartyAccountId = source.Id,
                Amount = amount,
                Currency = target.Currency,
                Type = TransactionType.Credit,
                Date = now,
                Description = description ?? $"Transfer from {fromId}"
            };

            source.Balance -= amount;
            target.Balance += amount;

            _context.Transactions.AddRange(debitTx, creditTx);
            await _context.SaveChangesAsync();

            return Ok();
        }

        // 7. Выписка по счёту (транзакции)
        [HttpGet("{accountId}/statement")]
        public async Task<ActionResult<IEnumerable<Transaction>>> GetStatement(Guid accountId)
        {
            var accountExists = await _context.Accounts.AnyAsync(a => a.Id == accountId);
            if (!accountExists) return NotFound();

            var transactions = await _context.Transactions
                .Where(t => t.AccountId == accountId)
                .OrderByDescending(t => t.Date)
                .ToListAsync();

            return transactions;
        }

        // 8. Проверить наличие счёта у клиента
        [HttpGet("exists/by-client/{clientId}")]
        public async Task<ActionResult<bool>> ClientHasAccount(Guid clientId)
        {
            var exists = await _context.Accounts.AnyAsync(a => a.OwnerId == clientId);
            return Ok(exists);
        }
    }

    public record TransferRequest
    {
        public Guid FromAccountId { get; init; }
        public Guid ToAccountId { get; init; }
        public decimal Amount { get; init; }
        public string? Description { get; init; }
    }
}
