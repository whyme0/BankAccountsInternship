using Api.Abstractions;
using Api.Features.Accounts;
// ReSharper disable PropertyCanBeMadeInitOnly.Global

namespace Api.Features.Transactions.GetStatement;

public class GetStatementQuery : IQuery<IEnumerable<TransactionDto>>
{
    public Guid AccountId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}