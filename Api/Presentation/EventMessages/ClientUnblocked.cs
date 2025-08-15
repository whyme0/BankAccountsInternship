namespace Api.Presentation.MessageEvents
{
    public class ClientUnblocked
    {
        public Guid EventId { get; set; }
        public DateTime OccuredAt { get; set; }
        public Guid ClientId { get; set; }
    }
}
