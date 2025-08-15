using Api.Presentation.MessageEvents;

namespace Api.Presentation.EventMessages
{
    public interface IMessageEvent<TPayload>
    {
        public Guid EventId { get; set; }
        public DateTime OccurredAt { get; set; }
        public Meta Meta { get; set; }
        public TPayload Payload { get; set; }
    }
}
