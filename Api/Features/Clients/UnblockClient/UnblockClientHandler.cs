using Api.Abstractions;
using Api.Data;
using Api.Exceptions;
using Api.Features.Clients.BlockClient;
using Api.Models;
using Api.Presentation.MessageEvents;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Api.Features.Clients.UnblockClient
{
    public class UnblockClientHandler(IAppDbContext context) : ICommandHandler<UnblockClientCommand, Unit>
    {
        public async Task<Unit> Handle(UnblockClientCommand request, CancellationToken cancellationToken)
        {
            var client = await context.Clients
                .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);
            if (client == null) throw new NotFoundException();

            var occuredAt = DateTime.UtcNow;
            var outbox = new Outbox
            {
                Id = Guid.NewGuid(),
                OccurredAt = occuredAt,
                Type = "ClientUnblocked",
                RoutingKey = "client.unblocked",
                Payload = JsonSerializer.Serialize(new ClientBlocked
                {
                    EventId = Guid.NewGuid(),
                    OccuredAt = occuredAt,
                    ClientId = client.Id
                })
            };

            context.Outbox.Add(outbox);
            await context.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}
