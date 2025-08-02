using Api.Models;
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
// ReSharper disable PreferConcreteValueOverDefault

namespace Api.Features.Transactions;

/// <summary>
/// Объект который представляет параметры, необходимые для создания транзакции
/// </summary>
public class CreateTransactionDto
{
    /// <summary>
    /// Счет, к которому относится тип `TransactionType` транзакции
    /// </summary>
    public Guid AccountId { get; set; }
    /// <summary>
    /// Опциональный параметр счета, используется только в случае перевода между счетами и выступает в роли получателя
    /// </summary>
    public Guid? CounterPartyAccountId { get; set; }
    /// <summary>
    /// Сумма, отражающая количество снимаемых/пополняемых средств
    /// </summary>
    public decimal Amount { get; set; }
    /// <summary>
    /// Валюта, в которой создается транзакция для счета
    /// </summary>
    public string Currency { get; set; } = default!;
    /// <summary>
    /// Отражает тип транзакции, а именно:<br/>
    /// Снятие - 0 (Credit)<br/>
    /// Пополнение - 1 (Debit)
    /// </summary>
    public TransactionType Type { get; set; }
    /// <summary>
    /// Опциональное сообщение для транзакции
    /// </summary>
    public string? Description { get; set; }
}