using Api.Models;
// ReSharper disable PreferConcreteValueOverDefault

namespace Api.Presentation.MessageEvents
{
    public class AccountOpened
    {
        public Guid EventId { get; set; }
        public DateTime OccuredAt { get; set; }
        public Guid AccountId { get; set; }
        public Guid OwnerId { get; set; }
        public string Currency { get; set; } = default!;
        public AccountType Type { get; set; }
    }
}
