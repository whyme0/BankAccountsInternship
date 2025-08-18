using System.Text;
using System.Text.Json;
using Api.Data;
using Api.Models;
using Api.Presentation.EventMessages;
using Api.Presentation.MessageEvents;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Api.Consumers
{
    public class AntifraudConsumer(ILogger<AntifraudConsumer> logger, IConnection rmqConnection, IServiceScopeFactory scopeFactory) : BackgroundService
    {
        protected override async Task<Task> ExecuteAsync(CancellationToken cancellationToken)
        {
            var channel = await rmqConnection.CreateChannelAsync(cancellationToken: cancellationToken);

            await channel.BasicQosAsync(0, 1, global: false, cancellationToken);

            await channel.QueueDeclareAsync("account.antifraud", durable: true, exclusive: false, autoDelete: false, cancellationToken: cancellationToken);
            await channel.QueueBindAsync("account.antifraud", "account.events", "client.*", cancellationToken: cancellationToken);

            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.ReceivedAsync += async (_, ea) =>
            {
                using var scope = scopeFactory.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var proceedEvent = true;

                try
                {
                    var body = Encoding.UTF8.GetString(ea.Body.ToArray());
                    var props = ea.BasicProperties;

                    var messageId = Guid.Parse(props.MessageId);

                    var inboxes = await context.InboxConsumed
                        .AnyAsync(x => x.Id == messageId, cancellationToken);

                    if (inboxes)
                    {
                        logger.LogInformation("Сообщение {MessageId} уже обработано", messageId);
                        await channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: true, cancellationToken: cancellationToken);
                        return;
                    }

                    switch (ea.RoutingKey)
                    {
                        case "client.blocked":
                            {
                                var evt = JsonSerializer.Deserialize<EventMessage<ClientBlocked>>(body);
                                if (evt != null)
                                {
                                    var client = await context.Clients.FirstOrDefaultAsync(c => c.Id == evt.Payload.ClientId, cancellationToken);

                                    client!.Frozen = true;

                                    context.Clients.Update(client);
                                    await context.SaveChangesAsync(cancellationToken);
                                    logger.LogInformation("Client {ClientId} заблокирован", evt.Payload.ClientId);
                                }
                                break;
                            }
                        case "client.unblocked":
                            {
                                var evt = JsonSerializer.Deserialize<EventMessage<ClientUnblocked>>(body);
                                if (evt != null)
                                {
                                    var client = await context.Clients.FirstOrDefaultAsync(c => c.Id == evt.Payload.ClientId, cancellationToken);

                                    client!.Frozen = false;

                                    context.Clients.Update(client);
                                    await context.SaveChangesAsync(cancellationToken);
                                    logger.LogInformation("Client {ClientId} разблокирован", evt.Payload.ClientId);
                                }
                                break;
                            }
                        default:
                            proceedEvent = false;
                            logger.LogWarning("Неизвестный routingKey: {RoutingKey}", ea.RoutingKey);
                            break;
                    }

                    if (proceedEvent)
                    {
                        context.AuditEvents.Add(new AuditEvent
                        {
                            Id = Guid.NewGuid(),
                            Payload = body,
                            RecivedAt = DateTime.UtcNow,
                            RoutingKey = ea.RoutingKey
                        });

                        context.InboxConsumed.Add(new InboxConsumed
                        {
                            Id = messageId,
                            ProcessedAt = DateTime.UtcNow,
                            Handler = nameof(AntifraudConsumer)
                        });
                        await context.SaveChangesAsync(cancellationToken);
                    }

                    await channel.BasicAckAsync(ea.DeliveryTag, multiple: false, cancellationToken: cancellationToken);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Ошибка при обработке сообщения");
                    await channel.BasicAckAsync(ea.DeliveryTag, multiple: false, cancellationToken: cancellationToken);
                }
            };

            await channel.BasicConsumeAsync("account.antifraud", autoAck: false, consumer: consumer, cancellationToken: cancellationToken);

            return Task.CompletedTask;
        }
    }
}
