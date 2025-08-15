namespace Api.Presentation.MessageEvents
{
    public class ClientBlocked
    {
        public Guid EventId { get; set; }
        public DateTime OccuredAt { get; set; }
        public Guid ClientId { get; set; }
    }
}
