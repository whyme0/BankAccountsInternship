using Api.Presentation.EventMessages;
using Api.Presentation.MessageEvents;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Diagnostics.Tracing;

namespace Api.Filters
{
    public class EventsDocumentFilter : IDocumentFilter
    {
        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            var events = new OpenApiPaths();

            var eventsTag = new OpenApiTag { Name = "Events", Description = "События, публикуемые в RabbitMQ"};

            if (swaggerDoc.Tags == null)
            {
                swaggerDoc.Tags = new List<OpenApiTag>();
            }
            if (swaggerDoc.Tags.All(t => t.Name != eventsTag.Name))
            {
                swaggerDoc.Tags.Add(eventsTag);
            }

            events.Add("account.opened", new OpenApiPathItem
            {
                Operations =
            {
                [OperationType.Get] = new OpenApiOperation
                {
                    Tags = new List<OpenApiTag> { eventsTag },
                    Summary = "Событие публикации при открытии счета",
                    Description = "Событие при открытии нового счета",
                    Responses = new OpenApiResponses
                    {
                        ["200"] = new OpenApiResponse
                        {
                            Description = "Пример сообщения",
                            Content =
                            {
                                ["application/json"] = new OpenApiMediaType
                                {
                                    Schema = context.SchemaGenerator.GenerateSchema(typeof(EventMessage<AccountOpened>), context.SchemaRepository),
                                }
                            }
                        }
                    }
                }
            }
            });

            events.Add("client.blocked", new OpenApiPathItem
            {
                Operations =
            {
                [OperationType.Get] = new OpenApiOperation
                {
                    Tags = new List<OpenApiTag> { eventsTag },
                    Summary = "Событие блокировки клиента",
                    Description = "Событие при блокировке клиента",
                    Responses = new OpenApiResponses
                    {
                        ["200"] = new OpenApiResponse
                        {
                            Description = "Пример сообщения",
                            Content =
                            {
                                ["application/json"] = new OpenApiMediaType
                                {
                                    Schema = context.SchemaGenerator.GenerateSchema(typeof(EventMessage<ClientBlocked>), context.SchemaRepository),
                                }
                            }
                        }
                    }
                }
            }
            });

            events.Add("client.unblocked", new OpenApiPathItem
            {
                Operations =
                {
                    [OperationType.Get] = new OpenApiOperation
                    {
                        Tags = new List<OpenApiTag> { eventsTag },
                        Summary = "Событие разблокировки клиента",
                        Description = "Событие при разблокировке клиента",
                        Responses = new OpenApiResponses
                        {
                            ["200"] = new OpenApiResponse
                            {
                                Description = "Пример сообщения",
                                Content =
                                {
                                    ["application/json"] = new OpenApiMediaType
                                    {
                                        Schema = context.SchemaGenerator.GenerateSchema(typeof(EventMessage<ClientUnblocked>), context.SchemaRepository),
                                    }
                                }
                            }
                        }
                    }
                }
            });

            events.Add("money.credited", new OpenApiPathItem
            {
                Operations =
                {
                    [OperationType.Get] = new OpenApiOperation
                    {
                        Tags = new List<OpenApiTag> { eventsTag },
                        Summary = "Событие зачисления средств",
                        Description = "Событие при зачислении средств на счёт",
                        Responses = new OpenApiResponses
                        {
                            ["200"] = new OpenApiResponse
                            {
                                Description = "Пример сообщения",
                                Content =
                                {
                                    ["application/json"] = new OpenApiMediaType
                                    {
                                        Schema = context.SchemaGenerator.GenerateSchema(typeof(EventMessage<MoneyCredited>), context.SchemaRepository),
                                    }
                                }
                            }
                        }
                    }
                }
            });

            events.Add("money.debited", new OpenApiPathItem
            {
                Operations =
                {
                    [OperationType.Get] = new OpenApiOperation
                    {
                        Tags = new List<OpenApiTag> { eventsTag },
                        Summary = "Событие списания средств",
                        Description = "Событие при списании средств со счёта",
                        Responses = new OpenApiResponses
                        {
                            ["200"] = new OpenApiResponse
                            {
                                Description = "Пример сообщения",
                                Content =
                                {
                                    ["application/json"] = new OpenApiMediaType
                                    {
                                        Schema = context.SchemaGenerator.GenerateSchema(typeof(EventMessage<MoneyDebited>), context.SchemaRepository),
                                    }
                                }
                            }
                        }
                    }
                }
            });

            events.Add("money.transfer.completed", new OpenApiPathItem
            {
                Operations =
                {
                    [OperationType.Get] = new OpenApiOperation
                    {
                        Tags = new List<OpenApiTag> { eventsTag },
                        Summary = "Событие завершённого перевода",
                        Description = "Событие при успешном завершении перевода между счетами",
                        Responses = new OpenApiResponses
                        {
                            ["200"] = new OpenApiResponse
                            {
                                Description = "Пример сообщения",
                                Content =
                                {
                                    ["application/json"] = new OpenApiMediaType
                                    {
                                        Schema = context.SchemaGenerator.GenerateSchema(typeof(EventMessage<TransferCompleted>), context.SchemaRepository),
                                    }
                                }
                            }
                        }
                    }
                }
            });

            events.Add("account.interest.accrued", new OpenApiPathItem
            {
                Operations =
                {
                    [OperationType.Get] = new OpenApiOperation
                    {
                        Tags = new List<OpenApiTag> { eventsTag },
                        Summary = "Событие начисления процентов",
                        Description = "Событие при начислении процентов по счёту",
                        Responses = new OpenApiResponses
                        {
                            ["200"] = new OpenApiResponse
                            {
                                Description = "Пример сообщения",
                                Content =
                                {
                                    ["application/json"] = new OpenApiMediaType
                                    {
                                        Schema = context.SchemaGenerator.GenerateSchema(typeof(EventMessage<InterestAccrued>), context.SchemaRepository),
                                    }
                                }
                            }
                        }
                    }
                }
            });

            foreach (var path in events)
            {
                swaggerDoc.Paths.Add(path.Key, path.Value);
            }
        }
    }
}
