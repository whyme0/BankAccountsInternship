using Api.Abstractions;

namespace Api.Features.Accounts.UpdateAccount
{
    public class UpdateAccountCommand : ICommand<AccountDto>
    {
        public Guid Id { get; set; }
        public decimal? InterestRate { get; set; }
        public DateTime? ClosedDate { get; set; }
    }
}
