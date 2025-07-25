using Api.Abstractions;
using MediatR;

namespace Api.Features.Accounts.DeleteAccount
{
    public class DeleteAccountCommand : ICommand
    {
        public Guid Id { get; set; }
    }
}
