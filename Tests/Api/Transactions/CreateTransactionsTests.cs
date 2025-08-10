using Api.Data;
using Api.Exceptions;
using Api.Features.Transactions.CreateTransaction;
using Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;

namespace Tests.Api.Transactions;

public class CreateTransactionsTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithDatabase("TestDb")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    private IServiceProvider _serviceProvider = null!;

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();
        var connectionString = _postgres.GetConnectionString();

        var services = new ServiceCollection();
        services.AddDbContext<AppDbContext>(o => o.UseNpgsql(connectionString));
        services.AddLogging();

        _serviceProvider = services.BuildServiceProvider();

        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await context.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        await _postgres.StopAsync();
    }

    [Fact]
    public async Task Handle_ValidDebitTransaction_ReturnsCreatedTransaction()
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var client = new Client { Id = Guid.NewGuid(), Name = "Test Client" };
        var account = new Account
        {
            Id = Guid.NewGuid(),
            OwnerId = client.Id,
            Type = 0,
            Balance = 1000,
            Currency = "RUB",
            OpenedDate = DateTime.UtcNow
        };

        context.Clients.Add(client);
        context.Accounts.Add(account);
        await context.SaveChangesAsync();

        var handler = new CreateTransactionHandler(context);
        var command = new CreateTransactionCommand
        {
            AccountId = account.Id,
            Amount = 500,
            Currency = "RUB",
            Type = TransactionType.Debit,
            Description = "Test debit"
        };

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(account.Id, result.AccountId);
        Assert.Equal(500, result.Amount);
        Assert.Equal(TransactionType.Debit, result.Type);
        Assert.Equal("Test debit", result.Description);

        var updatedAccount = await context.Accounts.FirstAsync(a => a.Id == account.Id);
        Assert.Equal(1500, updatedAccount.Balance);
    }

    [Fact]
    public async Task Handle_ValidCreditTransaction_ReturnsCreatedTransaction()
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var client = new Client { Id = Guid.NewGuid(), Name = "Test Client" };
        var account = new Account
        {
            Id = Guid.NewGuid(),
            OwnerId = client.Id,
            Type = 0,
            Balance = 1000,
            Currency = "RUB",
            OpenedDate = DateTime.UtcNow
        };

        context.Clients.Add(client);
        context.Accounts.Add(account);
        await context.SaveChangesAsync();

        var handler = new CreateTransactionHandler(context);
        var command = new CreateTransactionCommand
        {
            AccountId = account.Id,
            Amount = 300,
            Currency = "RUB",
            Type = TransactionType.Credit,
            Description = "Test credit"
        };

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(account.Id, result.AccountId);
        Assert.Equal(300, result.Amount);
        Assert.Equal(TransactionType.Credit, result.Type);

        var updatedAccount = await context.Accounts.FirstAsync(a => a.Id == account.Id);
        Assert.Equal(700, updatedAccount.Balance);
    }

    [Fact]
    public async Task Handle_AccountNotFound_Throws404NotFoundException()
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var handler = new CreateTransactionHandler(context);

        var command = new CreateTransactionCommand
        {
            AccountId = Guid.NewGuid(),
            Amount = 100,
            Currency = "RUB",
            Type = TransactionType.Debit
        };

        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_InvalidTransactionType_Throws400BadRequestException()
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var client = new Client { Id = Guid.NewGuid(), Name = "Test Client" };
        var account = new Account
        {
            Id = Guid.NewGuid(),
            OwnerId = client.Id,
            Type = 0,
            Balance = 1000,
            Currency = "RUB",
            OpenedDate = DateTime.UtcNow
        };

        context.Clients.Add(client);
        context.Accounts.Add(account);
        await context.SaveChangesAsync();

        var handler = new CreateTransactionHandler(context);
        var command = new CreateTransactionCommand
        {
            AccountId = account.Id,
            Amount = 100,
            Currency = "RUB",
            Type = (TransactionType)999
        };

        await Assert.ThrowsAsync<BadRequestException>(() =>
            handler.Handle(command, CancellationToken.None));
    }
}
