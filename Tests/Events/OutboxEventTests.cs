using Api.Data;
using Api.Models;
using Api.Presentation.EventMessages;
using HangfireJobs.Jobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using RabbitMQ.Client;
using System.Text.Json;
using Testcontainers.PostgreSql;

namespace Tests.Events;

public class OutboxEventTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _pg = new PostgreSqlBuilder()
        .WithDatabase("ModuleBankTestDb")
        .WithUsername("pguser")
        .WithPassword("pguser")
        .Build();

    private IServiceProvider _services = null!;
    private Mock<IChannel> _mockChannel = null!;
    private Mock<IConnection> _mockConnection = null!;
    private bool _rmqAvailable;

    public async Task InitializeAsync()
    {
        await _pg.StartAsync();

        var services = new ServiceCollection();

        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(_pg.GetConnectionString()));
        services.AddScoped<IAppDbContext>(s => s.GetRequiredService<AppDbContext>());

        services.AddLogging();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());

        _mockChannel = new Mock<IChannel>();
        _mockConnection = new Mock<IConnection>();

        _mockConnection
            .Setup(c => c.CreateChannelAsync(It.IsAny<CreateChannelOptions?>(), It.IsAny<CancellationToken>()))
            .Returns(async () =>
            {
                if (!_rmqAvailable)
                    throw new Exception("RabbitMQ unavailable");
                return await Task.FromResult(_mockChannel.Object);
            });

        services.AddSingleton(_mockConnection.Object);
        services.AddScoped<OutboxDispatcherJob>();

        _services = services.BuildServiceProvider();

        using var scope = _services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        await _pg.DisposeAsync();
    }

    [Fact]
    public async Task OutboxPublishesAfterFailure()
    {
        using var scope = _services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var evt = new Outbox
        {
            Id = Guid.NewGuid(),
            OccurredAt = DateTime.UtcNow.AddSeconds(-5),
            Type = "AccountOpened",
            RoutingKey = "account.opened",
            Payload = JsonSerializer.Serialize(new EventMessage<AccountOpened>
            {
                EventId = Guid.NewGuid(),
                OccurredAt = DateTime.UtcNow,
                Payload = new AccountOpened { AccountId = Guid.NewGuid(), OwnerId = Guid.NewGuid(), Currency = "RUB", Type = 0 },
                Meta = new Meta()
            })
        };
        db.Outbox.Add(evt);
        await db.SaveChangesAsync();

        var job = scope.ServiceProvider.GetRequiredService<OutboxDispatcherJob>();

        _rmqAvailable = false;
        await Assert.ThrowsAsync<Exception>(() => job.Execute(CancellationToken.None));

        var notPublished = await db.Outbox.FirstAsync(x => x.Id == evt.Id);
        Assert.Null(notPublished.PublishedAt);

        _rmqAvailable = true;
        await job.Execute(CancellationToken.None);

        var updated = await db.Outbox.FirstAsync(x => x.Id == evt.Id);
        Assert.NotNull(updated.PublishedAt);
    }
}