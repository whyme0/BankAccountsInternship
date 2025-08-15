using Api.Models;

namespace Api.Presentation.MessageEvents
{
    public class MoneyCredited
    {
        public Guid EventId { get; set; }
        public DateTime OccuredAt { get; set; }
        public Guid AccountId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = default!;
        public Guid OperationId { get; set; }
    }
}
