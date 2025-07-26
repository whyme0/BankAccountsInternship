using Api.Abstractions;
using Api.Models;
// ReSharper disable PropertyCanBeMadeInitOnly.Global
// ReSharper disable PreferConcreteValueOverDefault

namespace Api.Features.Transactions.CreateTransaction;

public class CreateTransactionCommand : ICommand<Transaction>
{
    public Guid AccountId { get; set; }
    public Guid? CounterPartyAccountId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = default!;
    public TransactionType Type { get; set; }
    public string? Description { get; set; }
}