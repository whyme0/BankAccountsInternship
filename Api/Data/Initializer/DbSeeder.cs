using Api.Models;

namespace Api.Data.Initializer
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(AppDbContext context)
        {
            if (context.Clients.Any()) return;

            var client1 = new Client { Id = Guid.NewGuid(), Name = "Ivan Ivanov" };
            var client2 = new Client { Id = Guid.NewGuid(), Name = "Petr Petrov" };

            var account1 = new Account
            {
                Id = Guid.NewGuid(),
                OwnerId = client1.Id,
                Type = AccountType.Deposit,
                Currency = "RUB",
                Balance = 1500m,
                InterestRate = 12.5m,
                OpenedDate = DateTime.UtcNow,
                ClosedDate = default
            };

            var account2 = new Account
            {
                Id = Guid.NewGuid(),
                OwnerId = client2.Id,
                Type = AccountType.Checking,
                Currency = "RUB",
                Balance = 200m,
                InterestRate = 0m,
                OpenedDate = DateTime.UtcNow.AddMonths(-2),
                ClosedDate = default
            };

            var t1 = new Transaction
            {
                Id = Guid.NewGuid(),
                AccountId = account1.Id,
                Amount = 200m,
                Currency = "RUB",
                Type = TransactionType.Credit,
                Date = DateTime.UtcNow.AddDays(-5),
                Description = "Initial deposit"
            };

            var t2 = new Transaction
            {
                Id = Guid.NewGuid(),
                AccountId = account2.Id,
                CounterPartyAccountId = account1.Id,
                Amount = 50m,
                Currency = "RUB",
                Type = TransactionType.Debit,
                Date = DateTime.UtcNow.AddDays(-2),
                Description = "Credit card payment"
            };

            context.Clients.AddRange(client1, client2);
            context.Accounts.AddRange(account1, account2);
            context.Transactions.AddRange(t1, t2);

            await context.SaveChangesAsync();
        }
    }

}
