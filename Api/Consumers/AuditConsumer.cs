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
    public class AuditConsumer(ILogger<AuditConsumer> logger, IConnection rmqConnection, IServiceScopeFactory scopeFactory) : BackgroundService
    {
        protected override async Task<Task> ExecuteAsync(CancellationToken cancellationToken)
        {
            var channel = await rmqConnection.CreateChannelAsync(cancellationToken: cancellationToken);

            await channel.BasicQosAsync(0, 1, global: false, cancellationToken);

            await channel.QueueDeclareAsync("account.audit", durable: true, exclusive: false, autoDelete: false, cancellationToken: cancellationToken);
            await channel.QueueBindAsync("account.audit", "account.events", "money.*", cancellationToken: cancellationToken);
            await channel.QueueBindAsync("account.audit", "account.events", "account.*", cancellationToken: cancellationToken);

            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.ReceivedAsync += async (_, ea) =>
            {
                using var scope = scopeFactory.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

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
                        case "account.opened":
                            {
                                var evt = JsonSerializer.Deserialize<AccountOpened>(body);
                                if (evt != null)
                                {
                                    logger.LogInformation("Открыт счет: {0}", evt.AccountId);
                                }
                                break;
                            }
                        case "money.transfer.credited":
                            {
                                var evt = JsonSerializer.Deserialize<MoneyCredited>(body);
                                if (evt != null)
                                {
                                    logger.LogInformation("Со счета '{0}' списана сумма '{1}'", evt.AccountId, evt.Amount);
                                }
                                break;
                            }
                        case "money.transfer.debited":
                            {
                                var evt = JsonSerializer.Deserialize<MoneyDebited>(body);
                                if (evt != null)
                                {
                                    logger.LogInformation("На счет '{0}' зачислена сумма '{1}'", evt.AccountId, evt.Amount);
                                }
                                break;
                            }
                        case "money.transfer.completed":
                        {
                            var evt = JsonSerializer.Deserialize<TransferCompleted>(body);
                            if (evt != null)
                            {
                                logger.LogInformation("Осуществлен перевод со счета '{0}' на счет '{1}'", evt.SourceAccountId, evt.DestinationAccountId);
                            }
                            break;
                        }
                        default:
                            logger.LogWarning("Неизвестный routingKey: {RoutingKey}", ea.RoutingKey);
                            break;
                    }

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
                        Handler = nameof(AuditConsumer)
                    });
                    await context.SaveChangesAsync(cancellationToken);

                    await channel.BasicAckAsync(ea.DeliveryTag, multiple: false, cancellationToken: cancellationToken);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Ошибка при обработке сообщения");
                    await channel.BasicAckAsync(ea.DeliveryTag, multiple: false, cancellationToken: cancellationToken);
                }
            };

            await channel.BasicConsumeAsync("account.audit", autoAck: false, consumer: consumer, cancellationToken: cancellationToken);

            return Task.CompletedTask;
        }
    }
}
