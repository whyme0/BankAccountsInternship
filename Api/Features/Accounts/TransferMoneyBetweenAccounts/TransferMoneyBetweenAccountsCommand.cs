using Api.Abstractions;
using MediatR;

namespace Api.Features.Accounts.TransferMoneyBetweenAccounts
{
    public class TransferMoneyBetweenAccountsCommand : ICommand
    {
        public Guid SenderAccountId { get; set; }
        public Guid RecipientAccountId { get; set; }
        public decimal Amount { get; set; }
    }
}
