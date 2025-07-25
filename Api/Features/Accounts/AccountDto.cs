using Api.Models;

namespace Api.Features.Accounts
{
    public class AccountDto
    {
        public Guid Id { get; set; }
        public ClientDto Owner { get; set; } = default!;
        public AccountType Type { get; set; }
        public string Currency { get; set; } = default!;
        public decimal Balance { get; set; }
        public decimal InterestRate { get; set; }
        public DateTime OpenedDate { get; set; }
        public DateTime? ClosedDate { get; set; }
        public ICollection<TransactionDto> Transactions { get; set; } = new List<TransactionDto>();
    }
}
