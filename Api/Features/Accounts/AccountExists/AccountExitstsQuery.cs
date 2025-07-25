using Api.Abstractions;
using MediatR;

namespace Api.Features.Accounts.AccountExists
{
    public class AccountExitstsQuery : IQuery<bool>
    {
        public Guid Id { get; set; }
        public Guid OwnerId { get; set; }
    }
}
