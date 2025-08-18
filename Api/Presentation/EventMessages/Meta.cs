namespace Api.Presentation.EventMessages
{
    /// <summary>
    /// Мета информация события
    /// </summary>
    public class Meta
    {
        /// <summary>
        /// Версия
        /// </summary>
        public string Version { get; set; } = "v1";
        /// <summary>
        /// Источник сообщения
        /// </summary>
        public string Source { get; set; } = "account-service";
        /// <summary>
        /// Идентификатор входящего запроса
        /// </summary>
        public Guid CorrelationId { get; set; } = Guid.NewGuid();
        /// <summary>
        /// Идентификатор команды, которая создает событие
        /// </summary>
        public Guid CausationId { get; set; } = Guid.NewGuid();
    }
}
