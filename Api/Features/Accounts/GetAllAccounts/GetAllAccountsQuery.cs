using Api.Abstractions;
using Api.Models;
using MediatR;

namespace Api.Features.Accounts.GetAllAccounts
{
    public class GetAllAccountsQuery : IQuery<IEnumerable<AccountDto>>
    {
    }
}
