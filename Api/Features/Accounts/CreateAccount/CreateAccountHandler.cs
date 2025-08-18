using Api.Abstractions;
using Api.Data;
using Api.Exceptions;
using Api.Features.Accounts.GetAccount;
using Api.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Text.Json;
using Api.Presentation.EventMessages;

// ReSharper disable PreferConcreteValueOverDefault

namespace Api.Features.Accounts.CreateAccount;

public class CreateAccountHandler(IAppDbContext context, IMediator mediator) : ICommandHandler<CreateAccountCommand, AccountDto>
{
    public async Task<AccountDto> Handle(CreateAccountCommand request, CancellationToken cancellationToken)
    {
        
        var dbContext = (AppDbContext)context;
        var accountOwner = context.Clients.FirstOrDefault(c => c.Id == request.OwnerId);

        if (accountOwner == null) throw new NotFoundException(request.OwnerId.ToString());

        var conn = dbContext.Database.GetDbConnection();
        if (conn.State != ConnectionState.Open)
            await conn.OpenAsync(cancellationToken);

        await using var transaction = await conn.BeginTransactionAsync(cancellationToken);
        await dbContext.Database.UseTransactionAsync(transaction, cancellationToken);

        try
        {
            var account = new Account
            {
                Id = new Guid(),
                OwnerId = accountOwner.Id,
                Owner = accountOwner,
                Type = request.Type,
                Currency = request.Currency,
                Balance = request.Balance,
                InterestRate = request.InterestRate,
                OpenedDate = DateTime.UtcNow,
                ClosedDate = request.ClosedDate
            };

            context.Accounts.Add(account);

            var occuredAt = DateTime.UtcNow;
            var outbox = new Outbox
            {
                Id = Guid.NewGuid(),
                OccurredAt = occuredAt,
                Type = "AccountOpened",
                RoutingKey = "account.opened",
                Payload = JsonSerializer.Serialize(new EventMessage<AccountOpened>
                {
                    EventId = Guid.NewGuid(),
                    OccurredAt = occuredAt,
                    Payload = new AccountOpened
                    {
                        AccountId = account.Id,
                        OwnerId = accountOwner.Id,
                        Currency = request.Currency,
                        Type = request.Type
                    },
                    Meta = new Meta()
                })
            };

            context.Outbox.Add(outbox);

            await context.SaveChangesAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);

            return await mediator.Send(new GetAccountQuery
            {
                Id = account.Id
            }, cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}