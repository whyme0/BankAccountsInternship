using Api.Models;

// ReSharper disable PreferConcreteValueOverDefault

namespace Api.Presentation.EventMessages
{
    /// <summary>
    /// Событие открытия счета
    /// </summary>
    public class AccountOpened
    {
        /// <summary>
        /// Идентификатор счета
        /// </summary>
        public Guid AccountId { get; set; }
        /// <summary>
        /// Владелец счета
        /// </summary>
        public Guid OwnerId { get; set; }
        /// <summary>
        /// Валюта
        /// </summary>
        public string Currency { get; set; } = default!;
        /// <summary>
        /// Тип счета
        /// </summary>
        public AccountType Type { get; set; }
    }
}
