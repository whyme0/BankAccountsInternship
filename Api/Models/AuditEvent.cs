namespace Api.Models
{
    public class AuditEvent
    {
        public Guid Id { get; set; }
        public DateTime RecivedAt { get; set; }
        public string RoutingKey { get; set; }
        public string Payload { get; set; }
    }
}
