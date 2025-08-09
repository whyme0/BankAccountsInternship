using Api.Data;
using Api.Features.Accounts.TransferMoneyBetweenAccounts;
using Api.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Testcontainers.PostgreSql;

namespace Tests.Api.Accounts;

public class TransferMoneyHandlerConcurrencyTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithDatabase("ModuleBankTestDb")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    private IServiceProvider _serviceProvider = null!;

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();
        var connectionString = _postgres.GetConnectionString();
        var services = new ServiceCollection();

        services.AddDbContext<IAppDbContext, AppDbContext>(o => o.UseNpgsql(connectionString));
        services.AddMediatR(c =>
            c.RegisterServicesFromAssemblies(typeof(TransferMoneyBetweenAccountsHandler).Assembly));
        services.AddLogging();

        _serviceProvider = services.BuildServiceProvider();

        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await context.Database.EnsureCreatedAsync();

        var ownerId = Guid.NewGuid();
        context.Clients.Add(new Client { Id = ownerId, Name = "Test Client" });
        context.Accounts.Add(new Account { Id = Guid.NewGuid(), OwnerId = ownerId, Type = 0, Balance = 1000, Currency = "RUB" });
        context.Accounts.Add(new Account { Id = Guid.NewGuid(), OwnerId = ownerId, Type = 0, Balance = 500, Currency = "RUB" });

        await context.SaveChangesAsync();
    }

    public async Task DisposeAsync()
    {
        await _postgres.StopAsync();
    }

    [Fact]
    public async Task Parallel_Transfers_Cause_Concurrency_Conflict_For_One_Request()
    {
        Guid senderId;
        Guid recipientId;

        using (var scope = _serviceProvider.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var accounts = await db.Accounts.ToListAsync();
            senderId = accounts[0].Id;
            recipientId = accounts[1].Id;
        }

        var cmd = new TransferMoneyBetweenAccountsCommand
        {
            SenderAccountId = senderId,
            RecipientAccountId = recipientId,
            Amount = 100
        };

        using var scope1 = _serviceProvider.CreateScope();
        using var scope2 = _serviceProvider.CreateScope();

        var mediator1 = scope1.ServiceProvider.GetRequiredService<IMediator>();
        var mediator2 = scope2.ServiceProvider.GetRequiredService<IMediator>();

        var task1 = Task.Run(() => mediator1.Send(cmd, CancellationToken.None));
        var task2 = Task.Run(() => mediator2.Send(cmd, CancellationToken.None));

        var results = await Task.WhenAll(CatchException(task1), CatchException(task2));

        Assert.Single(results, r => r is PostgresException { SqlState: "40001" });
    }

    private static async Task<Exception?> CatchException(Task task)
    {
        try { await task; return null; }
        catch (Exception ex) { return ex.GetBaseException(); }
    }
}