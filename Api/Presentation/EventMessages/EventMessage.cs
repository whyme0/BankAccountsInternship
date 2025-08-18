namespace Api.Presentation.EventMessages
{
    /// <summary>
    /// Обертка для всех событий
    /// </summary>
    /// <typeparam name="TPayload">Тип события</typeparam>
    public class EventMessage<TPayload> : IEventMessage<TPayload>
    {
        /// <summary>
        /// Идентификатор события
        /// </summary>
        public Guid EventId { get; set; }
        /// <summary>
        /// Дата возникновения события
        /// </summary>
        public DateTime OccurredAt { get; set; }
        /// <summary>
        /// Мета информация события
        /// </summary>
        public Meta Meta { get; set; }
        public TPayload Payload { get; set; }
    }
}
