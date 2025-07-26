using Api.Abstractions;

namespace Api.Features.Accounts.GetAllAccounts;

public class GetAllAccountsQuery : IQuery<IEnumerable<AccountDto>>;