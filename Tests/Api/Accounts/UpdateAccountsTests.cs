using System.Net;
using System.Text;
using System.Text.Json;
using Api.Data;
using Api.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace Tests.Api.Accounts;

public class UpdateAccountsTests(WebApplicationFactory<Program> factory)
    : IAsyncLifetime, IClassFixture<WebApplicationFactory<Program>>
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithDatabase("TestDb")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    private HttpClient _httpClient = null!;

    private IServiceProvider _serviceProvider = null!;

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

        services.AddDbContext<AppDbContext>(o => o.UseNpgsql(connectionString));
        services.AddLogging();

        _serviceProvider = services.BuildServiceProvider();

        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await context.Database.EnsureCreatedAsync();

        var ownerId = Guid.NewGuid();
        context.Clients.Add(new Client { Id = ownerId, Name = "Test Client" });
        context.Accounts.Add(new Account { Id = Guid.NewGuid(), OwnerId = ownerId, Type = 0, Balance = 1000, Currency = "RUB", ClosedDate = DateTime.UtcNow.AddDays(15) });

        await context.SaveChangesAsync();
    }

    public async Task DisposeAsync()
    {
        _httpClient.Dispose();
        await _postgres.StopAsync();
    }

    [Fact]
    public async Task Handle_ValidUpdateRequest_Returns200WithUpdatedAccount()
    {
        using var scope = _serviceProvider.CreateScope();

        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var account = db.Accounts.First();

        var newClosedDate = DateTime.UtcNow.AddDays(30);
        var updateAccountPayload = JsonSerializer.Serialize(new
        {
            ClosedDate = newClosedDate
        });

        var result = await _httpClient.PatchAsync($"/api/accounts/{account.Id}",
            new StringContent(updateAccountPayload, Encoding.UTF8, "application/json"));

        await db.Entry(account).ReloadAsync();

        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        Assert.Equal(
            account.ClosedDate!.Value.ToString("yyyy-MM-dd"),
            newClosedDate.ToString("yyyy-MM-dd"));
    }

    [Fact]
    public async Task Handle_AccountNotFound_Throws404NotFoundException()
    {
        using var scope = _serviceProvider.CreateScope();

        scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var newClosedDate = DateTime.UtcNow.AddDays(30);
        var updateAccountPayload = JsonSerializer.Serialize(new
        {
            ClosedDate = newClosedDate
        });

        var result = await _httpClient.PatchAsync($"/api/accounts/{Guid.NewGuid()}",
            new StringContent(updateAccountPayload, Encoding.UTF8, "application/json"));

        Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
    }

    [Fact]
    public async Task Handle_NoChanges_Returns304Null()
    {
        using var scope = _serviceProvider.CreateScope();

        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var accountId = db.Accounts.First().Id;

        var updateAccountPayload = JsonSerializer.Serialize(new
        {
            InterestRate = 0
        });

        var result = await _httpClient.PatchAsync($"/api/accounts/{accountId}",
            new StringContent(updateAccountPayload, Encoding.UTF8, "application/json"));

        Assert.Equal(HttpStatusCode.NotModified, result.StatusCode);
    }

    [Fact]
    public async Task Handle_AccountUpdate_BadPayload_Returns400()
    {
        using var scope = _serviceProvider.CreateScope();

        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var accountId = db.Accounts.First().Id;

        var updateAccountPayload = JsonSerializer.Serialize(new
        {
            InterestRate = 250
        });

        var result = await _httpClient.PatchAsync($"/api/accounts/{accountId}",
            new StringContent(updateAccountPayload, Encoding.UTF8, "application/json"));

        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
    }
}