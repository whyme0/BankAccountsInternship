using Api.Abstractions;
// ReSharper disable PropertyCanBeMadeInitOnly.Global

namespace Api.Features.Accounts.AccountExists;

public class AccountExistsQuery : IQuery<bool>
{
    public Guid Id { get; set; }
    public Guid OwnerId { get; set; }
}