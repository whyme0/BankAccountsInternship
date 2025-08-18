// ReSharper disable PropertyCanBeMadeInitOnly.Global
// ReSharper disable PreferConcreteValueOverDefault
// ReSharper disable UnusedAutoPropertyAccessor.Global
namespace Api.Presentation.EventMessages;

/// <summary>
/// Событие кредитования счета
/// </summary>
public class MoneyCredited
{
    /// <summary>
    /// Идентификатор счета
    /// </summary>
    public Guid AccountId { get; set; }
    /// <summary>
    /// Сумма снятия
    /// </summary>
    public decimal Amount { get; set; }
    /// <summary>
    /// Валюта
    /// </summary>
    public string Currency { get; set; } = default!;
    /// <summary>
    /// Идентификатор операции
    /// </summary>
    public Guid OperationId { get; set; }
}