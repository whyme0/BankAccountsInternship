using Api.Models;

namespace Api.Features.Transactions
{
    public class CreateTransactionDto
    {
        public Guid AccountId { get; set; }
        public Guid? CounterPartyAccountId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = default!;
        public TransactionType Type { get; set; }
        public string? Description { get; set; }
    }
}
