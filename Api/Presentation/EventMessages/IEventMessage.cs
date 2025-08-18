using Api.Presentation.MessageEvents;

namespace Api.Presentation.EventMessages
{
    public interface IEventMessage<TPayload> : IEmptyEventMessage
    {
        /// <summary>
        /// Содержимое события, соответствующее типу события
        /// </summary>
        public TPayload Payload { get; set; }
    }
}
