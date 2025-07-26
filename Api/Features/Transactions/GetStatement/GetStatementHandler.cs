using Api.Abstractions;
using Api.Data;
using Api.Exceptions;
using Api.Features.Accounts;
using Api.Features.Accounts.GetAccount;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Api.Features.Transactions.GetStatement
{
    public class GetStatementHandler : IQueryHandler<GetStatementQuery, IEnumerable<TransactionDto>>
    {
        private readonly IAppDbContext _context;
        private readonly IMediator _mediator;

        public GetStatementHandler(IAppDbContext context, IMediator mediator)
        {
            _context = context;
            _mediator = mediator;
        }

        public async Task<IEnumerable<TransactionDto>> Handle(GetStatementQuery request, CancellationToken cancellationToken)
        {
            var account = await _mediator.Send(new GetAccountQuery { Id = request.AccountId }, cancellationToken);

            if (account == null) throw new NotFoundException(request.AccountId.ToString());
            if (request.StartDate > request.EndDate) throw new BadRequestException("Wrong date range");

            var transactions = _context.Transactions
                .Where(t => t.AccountId == request.AccountId && t.Date >= request.StartDate &&
                            t.Date <= request.EndDate)
                .OrderByDescending(t => t.Date);
            
            return await transactions.Select(t => new TransactionDto()
            {
                Id = t.Id,
                AccountId = t.AccountId,
                CounterPartyAccountId = t.CounterPartyAccountId,
                Amount = t.Amount,
                Currency = t.Currency,
                Type = t.Type,
                Description = t.Description,
                Date = t.Date
            }).ToListAsync(cancellationToken);
        }
    }
}
