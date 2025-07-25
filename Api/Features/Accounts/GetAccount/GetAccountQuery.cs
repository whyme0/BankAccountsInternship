using Api.Abstractions;
using MediatR;

namespace Api.Features.Accounts.GetAccount
{
    public class GetAccountQuery : IQuery<AccountDto>
    {
        public Guid Id { get; set; }
    }
}
