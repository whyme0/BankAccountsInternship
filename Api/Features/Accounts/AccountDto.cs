using Api.Features.Clients;
using Api.Models;
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable PreferConcreteValueOverDefault

namespace Api.Features.Accounts;

/// <summary>
/// Модель представляет счета
/// </summary>
public class AccountDto
{
    /// <summary>
    /// Идентификатор счета
    /// </summary>
    public Guid Id { get; set; }
    /// <summary>
    /// Владелец счета
    /// </summary>
    public ClientDto Owner { get; set; } = default!;
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
    public ICollection<TransactionDto> Transactions { get; set; } = new List<TransactionDto>();
}