// ReSharper disable UnusedMember.Global
// ReSharper disable PropertyCanBeMadeInitOnly.Global
// ReSharper disable PreferConcreteValueOverDefault

using Swashbuckle.AspNetCore.Annotations;

namespace Api.Models;

/// <summary>
/// Счет клиента
/// </summary>
public class Account
{
    /// <summary>
    /// Идентификатор счета
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Идентификатор владельца
    /// </summary>
    public Guid OwnerId { get; set; }

    /// <summary>
    /// Владелец счета
    /// </summary>
    public Client Owner { get; set; } = default!;

    /// <summary>
    /// Тип счета:<br/>
    /// 0 - Текущий счет,<br/>
    /// 1 - Депозитный счет,<br/>
    /// 2 - Кредитный счет
    /// </summary>
    public AccountType Type { get; set; }

    /// <summary>
    /// Валюта
    /// </summary>
    public string Currency { get; set; } = default!;

    /// <summary>
    /// Баланс счета
    /// </summary>
    public decimal Balance { get; set; }

    /// <summary>
    /// Процентная ставка
    /// </summary>
    public decimal InterestRate { get; set; }

    /// <summary>
    /// Дата открытия счета
    /// </summary>
    public DateTime OpenedDate { get; set; }

    /// <summary>
    /// Дата закрытия счета
    /// </summary>
    public DateTime? ClosedDate { get; set; }

    /// <summary>
    /// Список всех транзакций
    /// </summary>
    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();

    [SwaggerIgnore]
    public uint Xmin { get; set; }
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