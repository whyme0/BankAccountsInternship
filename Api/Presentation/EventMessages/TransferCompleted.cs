using Api.Models;

namespace Api.Presentation.MessageEvents
{
    public class TransferCompleted
    {
        public Guid EventId { get; set; }
        public DateTime OccuredAt { get; set; }
        public Guid SourceAccountId { get; set; }
        public Guid DestinationAccountId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = default!;
        public Guid TransferId { get; set; }
    }
}
