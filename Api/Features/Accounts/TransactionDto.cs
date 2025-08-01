using Api.Models;
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable PreferConcreteValueOverDefault

namespace Api.Features.Accounts;

/// <summary>
/// Транзакция счета
/// </summary>
public class TransactionDto
{
    /// <summary>
    /// Идентификатор транзакции
    /// </summary>
    public Guid Id { get; set; }
    /// <summary>
    /// Идентификатор счета транзакции
    /// </summary>
    public Guid AccountId { get; set; }
    /// <summary>
    /// Идентификтор контрагента.<br/>
    /// Если AccountId - отправитель, контрагент является получателем. Такая транзакиця будет иметь тип "0".<br/>
    /// Если AccountId - получатель, контрагент является отправителем. Такая транзакиця будет иметь тип "1".<br/>
    /// <strong>Если транзакция была выполнена в процессе работы с банкоматом, то идентификатор контрагента закономерно отсутствует</strong>
    /// </summary>
    public Guid? CounterPartyAccountId { get; set; }
    /// <summary>
    /// Сумма перевода
    /// </summary>
    public decimal Amount { get; set; }
    /// <summary>
    /// Валюта транзакции
    /// </summary>
    public string Currency { get; set; } = default!;
    /// <summary>
    /// Тип счета:<br/>
    /// 0 - Снятие средств,<br/>
    /// 1 - Пополнение средства
    /// </summary>
    public TransactionType Type { get; set; }
    /// <summary>
    /// Текстовое описание/сообщение транзакции
    /// </summary>
    public string? Description { get; set; }
    /// <summary>
    /// Дата проведения транзакции, устанавливаемая автоматически
    /// </summary>
    public DateTime Date { get; set; }
}