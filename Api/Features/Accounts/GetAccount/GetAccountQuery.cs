using Api.Abstractions;
// ReSharper disable PropertyCanBeMadeInitOnly.Global

namespace Api.Features.Accounts.GetAccount;

public class GetAccountQuery : IQuery<AccountDto>
{
    public Guid Id { get; set; }
}