using Api.Models;

namespace Api.Presentation.MessageEvents
{
    /// <summary>
    /// События дебетирования счета
    /// </summary>
    public class MoneyDebited
    {
        /// <summary>
        /// Идентификатор счета
        /// </summary>
        public Guid AccountId { get; set; }
        /// <summary>
        /// Сумма пополнения
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
        /// <summary>
        /// Причина пополнения
        /// </summary>
        public string Reason { get; set; } = default!;
    }
}
