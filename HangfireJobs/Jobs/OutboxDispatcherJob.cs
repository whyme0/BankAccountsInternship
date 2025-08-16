
using Api.Data;
using RabbitMQ.Client;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace HangfireJobs.Jobs
{
    public class OutboxDispatcherJob(IConfiguration configuration, IConnection rabbitConnection, ILogger<OutboxDispatcherJob>? logger, IAppDbContext context) : IJob
    {
        public async Task Execute(CancellationToken cancellationToken)
        {
            var outboxes = await context.Outbox
                .Where(x => x.PublishedAt == null)
                .OrderBy(x => x.OccurredAt)
                .Take(50)
                .ToListAsync(cancellationToken);

            if (!outboxes.Any()) return;

            await using var channel = await rabbitConnection.CreateChannelAsync(cancellationToken: cancellationToken);

            
            foreach (var msg in outboxes)
            {
                try
                {
                    var body = Encoding.UTF8.GetBytes(msg.Payload);

                    var props = new BasicProperties
                    {
                        MessageId = msg.Id.ToString(),
                        ContentType = "application/json",
                        DeliveryMode = DeliveryModes.Persistent
                    };

                    await channel.BasicPublishAsync(
                        "account.events",
                        msg.RoutingKey,
                        true,
                        props,
                        body, cancellationToken);

                    msg.PublishedAt = DateTime.UtcNow;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, $"Ошибка при публикации Outbox-сообщения {msg.Id}");
                    throw;
                }
            }

            context.Outbox.UpdateRange(outboxes);
            await context.SaveChangesAsync(cancellationToken);
        }
    }
}
