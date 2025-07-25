using Api.Abstractions;
using Api.Features.Accounts;
using MediatR;

namespace Api.Features.Transactions.GetTransaction
{
    public class GetStatementQuery : IQuery<IEnumerable<TransactionDto>>
    {
        public Guid AccountId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
