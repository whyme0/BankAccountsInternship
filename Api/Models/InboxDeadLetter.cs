namespace Api.Models
{
    public class InboxDeadLetter
    {
        public Guid Id { get; set; }
        public string MessageId { get; set; }
        public DateTime ReceivedAt { get; set; }
        public string Handler { get; set; }
        public string Payload { get; set; }
        public string Error { get; set; }
    }
}
