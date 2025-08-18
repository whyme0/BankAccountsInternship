// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable UnusedMember.Global
namespace Api.Presentation.EventMessages;

/// <summary>
/// Событие начисления процентов
/// </summary>
public class InterestAccrued
{
    /// <summary>
    /// Идентификатор счета
    /// </summary>
    public Guid AccountId { get; set; }
    /// <summary>
    /// Дата начала срока счета
    /// </summary>
    public DateTime PeriodFrom { get; set; }
    /// <summary>
    /// Дата завершения срока счета
    /// </summary>
    public DateTime PeriodTo { get; set; }
    /// <summary>
    /// Сумма начисленная на счет
    /// </summary>
    public decimal Amount { get; set; }
}