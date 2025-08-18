namespace Api.Presentation.MessageEvents
{
    /// <summary>
    /// Событие разблокировки клиента
    /// </summary>
    public class ClientUnblocked
    {
        /// <summary>
        /// Идентификатор клиента
        /// </summary>
        public Guid ClientId { get; set; }
    }
}
