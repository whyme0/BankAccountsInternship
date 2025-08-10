using Api.Data;
using Api.Features.Accounts.TransferMoneyBetweenAccounts;
using Api.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Text;
using System.Text.Json;
using Testcontainers.PostgreSql;

namespace Tests.Api.Accounts;

public class ParallelTransferTests(WebApplicationFactory<Program> factory)
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
    public async Task ParallelTransferTests_50_Parallel_Transfers_Preserve_Total_Balance()
    {
        using var scope = _serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var accounts = await db.Accounts.ToListAsync();
        var senderId = accounts[0].Id;
        var recipientId = accounts[1].Id;
        var initialTotalBalance = accounts.Sum(a => a.Balance);

        var transferTasks = new List<Task<HttpResponseMessage>>();

        for (var i = 0; i < 50; i++)
        {
            var transferPayload = JsonSerializer.Serialize(new
            {
                RecipientId = recipientId,
                Amount = 700
            });

            var task = _httpClient.PostAsync($"/api/accounts/{senderId}/transfer",
                new StringContent(transferPayload, Encoding.UTF8, "application/json"));

            transferTasks.Add(task);
        }

        var responses = await Task.WhenAll(transferTasks);

        var createdTransfers = responses.Count(r => r.StatusCode == System.Net.HttpStatusCode.Created);
        var conflictResponses = responses.Count(r => r.StatusCode == System.Net.HttpStatusCode.Conflict);

        Assert.Equal(50, createdTransfers + conflictResponses);

        var finalTotalBalance = accounts.Sum(a => a.Balance);

        Assert.Equal(initialTotalBalance, finalTotalBalance);

        foreach (var response in responses)
        {
            response.Dispose();
        }
    }
}