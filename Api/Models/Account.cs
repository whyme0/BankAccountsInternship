// ReSharper disable UnusedMember.Global
// ReSharper disable PropertyCanBeMadeInitOnly.Global
// ReSharper disable PreferConcreteValueOverDefault

namespace Api.Models;

public class Account
{
    public Guid Id { get; set; }
    public Guid OwnerId { get; set; }
    public Client Owner { get; set; } = default!;
    public AccountType Type { get; set; }
    public string Currency { get; set; } = default!;
    public decimal Balance { get; set; }
    public decimal InterestRate { get; set; }
    public DateTime OpenedDate { get; set; }
    public DateTime? ClosedDate { get; set; }
    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}

/// <summary>
/// Тип счета:<br/>
/// 0 - Текущий счет,
/// 1 - Депозитный счет,
/// 2 - Кредитный счет
/// </summary>
public enum AccountType
{
    Checking,
    Deposit,
    Credit
}