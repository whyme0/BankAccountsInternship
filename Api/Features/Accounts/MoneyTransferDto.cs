// ReSharper disable UnusedAutoPropertyAccessor.Global
namespace Api.Features.Accounts;

/// <summary>
/// Объект представляет параметры перевода денежных средств между счетами
/// </summary>
public class MoneyTransferDto
{
    /// <summary>
    /// Идентификатор счета получателя
    /// </summary>
    public Guid RecipientId { get; set; }
    
    /// <summary>
    /// Сумма перевода
    /// </summary>
    public decimal Amount { get; set; }
}