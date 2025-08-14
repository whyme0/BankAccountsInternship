using Api.Data;
using Api.Features.Accounts.TransferMoneyBetweenAccounts;
using Api.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Text;
using System.Text.Json;
using Api.Exceptions;
using Testcontainers.PostgreSql;

namespace Tests.Api.Accounts;

public class TransferMoneyConcurrencyTests(WebApplicationFactory<Program> factory)
    : IAsyncLifetime, IClassFixture<WebApplicationFactory<Program>>
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithDatabase("ModuleBankTestDb")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    private IServiceProvider _serviceProvider = null!;
    private HttpClient _httpClient = null!;

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();
        var connectionString = _postgres.GetConnectionString();

        _httpClient = factory.WithWebHostBuilder(b =>
        {
            b.ConfigureServices(s =>
            {
                var descriptor = s.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
                if (descriptor != null) s.Remove(descriptor);
                s.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));
            });
        }).CreateClient();
        _httpClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", JwtTokenCreator.CreateTestJwtToken());

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
        _httpClient.Dispose();
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

        Assert.Single(results, r => r is SerializationConflictException);
    }

    [Fact]
    public async Task Parallel_Transfer_In_Api_Cause_409_Conflict_Response()
    {
        using var scope = _serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var accounts = await db.Accounts.ToListAsync();
        var sender = accounts[0];
        var recipient = accounts[1];

        var transferMoneyPayload = JsonSerializer.Serialize(new
        {
            RecipientId = recipient.Id,
            Amount = 100
        });


        var task1 = _httpClient.PostAsync($"/api/accounts/{sender.Id}/transfer",
            new StringContent(transferMoneyPayload, Encoding.UTF8, "application/json"));
        var task2 = _httpClient.PostAsync($"/api/accounts/{sender.Id}/transfer",
            new StringContent(transferMoneyPayload, Encoding.UTF8, "application/json"));

        var responses = await Task.WhenAll(task1, task2);
        var statusCodes = responses.Select(r => r.StatusCode).ToList();

        Assert.Contains(statusCodes, s => s == System.Net.HttpStatusCode.Conflict);
    }

    private static async Task<Exception?> CatchException(Task task)
    {
        try { await task; return null; }
        catch (Exception ex) { return ex.GetBaseException(); }
    }
}