using Api.Models;

namespace Api.Presentation.MessageEvents
{
    /// <summary>
    /// Событие выполнения перевода средств
    /// </summary>
    public class TransferCompleted
    {
        /// <summary>
        /// Идентификатор отправителя
        /// </summary>
        public Guid SourceAccountId { get; set; }
        /// <summary>
        /// Идентификатор отправителя
        /// </summary>
        public Guid DestinationAccountId { get; set; }
        /// <summary>
        /// Сумма
        /// </summary>
        public decimal Amount { get; set; }
        /// <summary>
        /// Валюта
        /// </summary>
        public string Currency { get; set; } = default!;
        /// <summary>
        /// Идентификатор перевода
        /// </summary>
        public Guid TransferId { get; set; }
    }
}
