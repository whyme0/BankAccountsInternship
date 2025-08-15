namespace Api.Presentation.EventMessages
{
    public class Meta
    {
        public string Version { get; set; } = "v1";
        public string Source { get; set; } = "account-service";
        public Guid CorrelationId { get; set; } = Guid.NewGuid();
        public Guid CausationId { get; set; } = Guid.NewGuid();
    }
}
