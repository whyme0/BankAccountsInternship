using Api.Models;
using Microsoft.EntityFrameworkCore;
// ReSharper disable UnusedMemberInSuper.Global

namespace Api.Data;

public interface IAppDbContext
{
    public DbSet<Client> Clients { get; set; }
    public DbSet<Account> Accounts { get; set; }
    public DbSet<Transaction> Transactions { get; set; }
    public DbSet<Outbox> Outbox { get; set; }
    public DbSet<InboxConsumed> InboxConsumed { get; set; }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}