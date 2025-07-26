using Api.Abstractions;
using MediatR;
// ReSharper disable PropertyCanBeMadeInitOnly.Global

namespace Api.Features.Accounts.DeleteAccount;

public class DeleteAccountCommand : ICommand<Unit>
{
    public Guid Id { get; set; }
}