using Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Api.Data
{
    public interface IAppDbContext
    {
        public DbSet<Client> Clients { get; set; }
        public DbSet<Account> Accounts { get; set; }
        public DbSet<Transaction> Transactions { get; set; }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
