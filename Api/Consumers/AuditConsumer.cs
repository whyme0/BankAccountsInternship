using System.Reflection.Metadata.Ecma335;
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
        protected override async Task<Task> ExecuteAsync(CancellationToken ct)
        {
            var channel = await rmqConnection.CreateChannelAsync(cancellationToken: ct);

            await channel.BasicQosAsync(0, 1, global: false, ct);

            await channel.QueueDeclareAsync("account.audit", durable: true, exclusive: false, autoDelete: false, cancellationToken: ct);
            await channel.QueueBindAsync("account.audit", "account.events", "money.*", cancellationToken: ct);
            await channel.QueueBindAsync("account.audit", "account.events", "account.*", cancellationToken: ct);

            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.ReceivedAsync += async (_, ea) =>
            {
                using var scope = scopeFactory.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var proceedEvent = true;
                int retryCount = ea.BasicProperties.Headers?.ContainsKey("x-retry") == true
                    ? (int)(ea.BasicProperties.Headers["x-retry"] ?? 0)
                    : 0;

                try
                {
                    var body = Encoding.UTF8.GetString(ea.Body.ToArray());
                    var props = ea.BasicProperties;

                    var messageId = Guid.Parse(props.MessageId);

                    EventMessage<object>? checkingEvt;
                    try
                    {
                        checkingEvt = JsonSerializer.Deserialize<EventMessage<object>>(body);
                    }
                    catch (JsonException ex)
                    {
                        context.InboxDeadLetters.Add(new InboxDeadLetter
                        {
                            Error = $"Неверный envelope: {ex.Message}",
                            Handler = nameof(AntifraudConsumer),
                            Payload = body,
                            Id = Guid.NewGuid(),
                            MessageId = props.MessageId,
                            ReceivedAt = DateTime.UtcNow
                        });

                        await context.SaveChangesAsync(ct);

                        logger.LogWarning(ex, "Invalid envelope for message {MessageId}", props.MessageId);
                        return;
                    }

                    if (checkingEvt.Meta.Version != "v1")
                    {
                        context.InboxDeadLetters.Add(new InboxDeadLetter()
                        {
                            Error = $"Данная версия (${checkingEvt.Meta.Version}) не поддерживается",
                            Handler = nameof(AntifraudConsumer),
                            Payload = body,
                            Id = Guid.NewGuid(),
                            MessageId = props.MessageId,
                            ReceivedAt = DateTime.UtcNow
                        });

                        await context.SaveChangesAsync(ct);

                        logger.LogWarning("Unsupported or missing meta.version for message {MessageId}", props.MessageId);
                        return;
                    }

                    var inboxes = await context.InboxConsumed
                        .AnyAsync(x => x.Id == messageId, ct);

                    if (inboxes)
                    {
                        logger.LogInformation("Сообщение {MessageId} уже обработано", messageId);
                        await channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: true, cancellationToken: ct);
                        return;
                    }
                    
                    switch (ea.RoutingKey)
                    {
                        case "account.opened":
                            {
                                var evt = JsonSerializer.Deserialize<EventMessage<AccountOpened>>(body);
                                if (evt != null)
                                {
                                    logger.LogInformation("Открыт счет: {0}", evt.Payload.AccountId);
                                }
                                break;
                            }
                        case "money.transfer.credited":
                            {
                                var evt = JsonSerializer.Deserialize<EventMessage<MoneyCredited>>(body);
                                if (evt != null)
                                {
                                    logger.LogInformation("Со счета '{0}' списана сумма '{1}'", evt.Payload.AccountId, evt.Payload.Amount);
                                }
                                break;
                            }
                        case "money.transfer.debited":
                            {
                                var evt = JsonSerializer.Deserialize<EventMessage<MoneyDebited>>(body);
                                if (evt != null)
                                {
                                    logger.LogInformation("На счет '{0}' зачислена сумма '{1}'", evt.Payload.AccountId, evt.Payload.Amount);
                                }
                                break;
                            }
                        case "money.transfer.completed":
                        {
                            var evt = JsonSerializer.Deserialize<EventMessage<TransferCompleted>>(body);
                            if (evt != null)
                            {
                                logger.LogInformation("Осуществлен перевод со счета '{0}' на счет '{1}'", evt.Payload.SourceAccountId, evt.Payload.DestinationAccountId);
                            }
                            break;
                        }
                        case "money.interest.accrued":
                        {
                            var evt = JsonSerializer.Deserialize<EventMessage<InterestAccrued>>(body);
                            if (evt != null)
                            {
                                logger.LogInformation("Начислен процент на счет '{0}'", evt.Payload.AccountId);
                            }
                            break;
                        }
                        default:
                            logger.LogWarning("Неизвестный routingKey: {RoutingKey}", ea.RoutingKey);
                            proceedEvent = false;
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
                            Handler = nameof(AuditConsumer)
                        });
                        await context.SaveChangesAsync(ct);

                        var evt = JsonSerializer.Deserialize<EventMessage<object>>(body);
                        logger.LogInformation("Обработано событие {0} {1} Correlation={2} Retry={3} Latency={4} ms",
                            evt!.EventId,
                            ea.RoutingKey,
                            evt.Meta.CorrelationId,
                            retryCount,
                            (DateTime.UtcNow - evt.OccurredAt).TotalMilliseconds);
                    }

                    await channel.BasicAckAsync(ea.DeliveryTag, multiple: false, cancellationToken: ct);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Ошибка при обработке сообщения");
                    await channel.BasicAckAsync(ea.DeliveryTag, multiple: false, cancellationToken: ct);
                }
            };

            await channel.BasicConsumeAsync("account.audit", autoAck: false, consumer: consumer, cancellationToken: ct);

            return Task.CompletedTask;
        }
    }
}
