using Api.Models;
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
// ReSharper disable PreferConcreteValueOverDefault

namespace Api.Features.Transactions;

public class CreateTransactionDto
{
    public Guid AccountId { get; set; }
    public Guid? CounterPartyAccountId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = default!;
    public TransactionType Type { get; set; }
    public string? Description { get; set; }
}