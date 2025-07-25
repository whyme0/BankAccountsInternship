using Api.Abstractions;
using Api.Models;
using MediatR;

namespace Api.Features.Accounts.UpdateAccount
{
    public class UpdateAccountCommand : ICommand<AccountDto>
    {
        public Guid Id { get; set; }
        public decimal? InterestRate { get; set; }
        public DateTime? ClosedDate { get; set; }
    }
}
