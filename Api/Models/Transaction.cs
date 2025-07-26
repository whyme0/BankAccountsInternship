// ReSharper disable PropertyCanBeMadeInitOnly.Global
// ReSharper disable PreferConcreteValueOverDefault
namespace Api.Models;

public class Transaction
{
    public Guid Id { get; set; }
    public Guid AccountId { get; set; }
    public Account Account { get; set; } = default!;
    public Guid? CounterPartyAccountId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = default!;
    public TransactionType Type { get; set; }
    public string? Description { get; set; }
    public DateTime Date { get; set; }
}

public enum TransactionType
{
    Credit,
    Debit
}