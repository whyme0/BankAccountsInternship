using Api.Abstractions;
using Api.Models;
using MediatR;

namespace Api.Features.Accounts.CreateAccount
{
    public class CreateAccountCommand : ICommand<AccountDto>
    {
        public Guid OwnerId { get; set; }
        public AccountType Type { get; set; }
        public string Currency { get; set; } = default!;
        public decimal Balance { get; set; }
        public decimal InterestRate { get; set; }
        public DateTime? ClosedDate { get; set; }
    }
}
