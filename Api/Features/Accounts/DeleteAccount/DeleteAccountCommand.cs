using Api.Abstractions;

namespace Api.Features.Accounts.DeleteAccount
{
    public class DeleteAccountCommand : ICommand
    {
        public Guid Id { get; set; }
    }
}
