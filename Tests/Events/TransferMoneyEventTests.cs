using Api.Data;
using Api.Features.Accounts.TransferMoneyBetweenAccounts;
using Api.Models;
using Docker.DotNet.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Org.BouncyCastle.Cms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Testcontainers.PostgreSql;

namespace Tests.Events
{
    public class TransferMoneyEventTests
        : IClassFixture<WebApplicationFactory<Program>>, IAsyncLifetime
    {
        private readonly PostgreSqlContainer _pg = new PostgreSqlBuilder()
        .WithDatabase("ModuleBankTestDb")
        .WithUsername("pguser")
        .WithPassword("pguser")
        .Build();

        private readonly WebApplicationFactory<Program> _originalFactory;

        private WebApplicationFactory<Program> _factory = null!;
        private HttpClient _client = null!;

        public TransferMoneyEventTests(WebApplicationFactory<Program> factory)
        {
            _originalFactory = factory;
        }

        public async Task InitializeAsync()
        {
            await _pg.StartAsync();

            _factory = _originalFactory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    var dbContextOptions = services.SingleOrDefault(
                        d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
                    if (dbContextOptions != null)
                    {
                        services.Remove(dbContextOptions);
                    }

                    services.AddDbContext<AppDbContext>(options =>
                    {
                        options.UseNpgsql(_pg.GetConnectionString());
                    });
                });
            });

            _client = _factory.CreateClient();
            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", JwtTokenCreator.CreateTestJwtToken());

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await db.Database.EnsureCreatedAsync();
        }

        public async Task DisposeAsync()
        {
            await _pg.DisposeAsync();
        }

        [Fact]
        public async Task TransferEmitsSingleEvent()
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var client = new Client { Id = Guid.NewGuid(), Name = "Test", Frozen = false };
            var acc1 = new Account { Id = Guid.NewGuid(), OwnerId = client.Id, Type = 0, Balance = 1000, Currency = "RUB" };
            var acc2 = new Account { Id = Guid.NewGuid(), OwnerId = client.Id, Type = 0, Balance = 1000, Currency = "RUB" };

            db.Clients.Add(client);
            db.Accounts.AddRange(acc1, acc2);
            await db.SaveChangesAsync();

            for (int i = 0; i < 50; i++)
            {
                var transferPayload = JsonSerializer.Serialize(new
                {
                    RecipientId = acc2.Id,
                    Amount = 1
                });

                var resp = await _client.PostAsync($"/api/accounts/{acc1.Id}/transfer",
                    new StringContent(transferPayload, Encoding.UTF8, "application/json"));

                if (!resp.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Error transferring money. Status code: {resp.StatusCode}");
                    var content = await resp.Content.ReadAsStringAsync();
                    Console.WriteLine($"Response body: {content}");
                }

                resp.EnsureSuccessStatusCode();
            }

            var count = await db.Outbox.CountAsync(o => o.Type == "TransferCompleted");
            Assert.Equal(50, count);
        }
    }

}
