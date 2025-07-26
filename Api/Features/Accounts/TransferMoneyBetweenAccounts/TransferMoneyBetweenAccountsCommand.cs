using Api.Abstractions;
using MediatR;
// ReSharper disable PropertyCanBeMadeInitOnly.Global

namespace Api.Features.Accounts.TransferMoneyBetweenAccounts;

public class TransferMoneyBetweenAccountsCommand : ICommand<Unit>
{
    public Guid SenderAccountId { get; set; }
    public Guid RecipientAccountId { get; set; }
    public decimal Amount { get; set; }
}