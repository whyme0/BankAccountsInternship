using Api.Models;
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
// ReSharper disable PreferConcreteValueOverDefault

namespace Api.Features.Accounts;

/// <summary>
/// Объект с параметрами для открытия счета 
/// </summary>
public class CreateAccountDto
{
    /// <summary>
    /// Идентификатор для которого открывается счет
    /// </summary>
    public Guid OwnerId { get; set; }

    /// <summary>
    /// Тип счета:<br/>
    /// 0 - Текущий счет,
    /// 1 - Депозитный счет,
    /// 2 - Кредитный счет
    /// </summary>
    public AccountType Type { get; set; }

    /// <summary>
    /// Валюта в формате iso 4217
    /// </summary>
    public string Currency { get; set; } = default!;

    /// <summary>
    /// Начальный баланс
    /// </summary>
    public decimal Balance { get; set; }

    /// <summary>
    /// Процентная ставка
    /// </summary>
    public decimal InterestRate { get; set; }

    /// <summary>
    /// Дата закрытия счета
    /// </summary>
    public DateTime? ClosedDate { get; set; }
}