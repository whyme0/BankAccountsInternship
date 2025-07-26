using Api.Abstractions;

namespace Api.Features.Accounts.AccountExists
{
    public class AccountExistsQuery : IQuery<bool>
    {
        public Guid Id { get; set; }
        public Guid OwnerId { get; set; }
    }
}
