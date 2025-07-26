namespace Api.Features.Accounts
{
    public class MoneyTransferDto
    {
        public Guid RecipientId { get; set; }
        public decimal Amount { get; set; }
    }
}
