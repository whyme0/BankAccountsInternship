using Api.Abstractions;
using Api.Data;
using Api.Exceptions;
using Api.Models;
using Api.Presentation.EventMessages;
using Api.Presentation.MessageEvents;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Security.Principal;
using System.Text.Json;

namespace Api.Features.Clients.BlockClient
{
    public class BlockClientHandler(IAppDbContext context) : ICommandHandler<BlockClientCommand, Unit>
    {
        public async Task<Unit> Handle(BlockClientCommand request, CancellationToken cancellationToken)
        {
            var client = await context.Clients
                .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);
            if (client == null) throw new NotFoundException();
            
            var occuredAt = DateTime.UtcNow;
            var outbox = new Outbox
            {
                Id = Guid.NewGuid(),
                OccurredAt = occuredAt,
                Type = "ClientBlocked",
                RoutingKey = "client.blocked",
                Payload = JsonSerializer.Serialize(new EventMessage<ClientBlocked>
                {
                    EventId = Guid.NewGuid(),
                    OccurredAt = occuredAt,
                    Payload = new ClientBlocked
                    {
                        ClientId = client.Id
                    },
                    Meta = new Meta()
                })
            };

            context.Outbox.Add(outbox);
            await context.SaveChangesAsync(cancellationToken);
            
            return Unit.Value;
        }
    }
}
