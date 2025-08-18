namespace Api.Presentation.MessageEvents
{
    /// <summary>
    /// Событие блокировки клиента
    /// </summary>
    public class ClientBlocked
    {
        /// <summary>
        /// Идентификатор клиента
        /// </summary>
        public Guid ClientId { get; set; }
    }
}
