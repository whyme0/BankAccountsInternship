using Api.Abstractions;
using Api.Data;
using Api.Exceptions;

namespace Api.Features.Accounts.DeleteAccount
{
    public class DeleteAccountHandler : ICommandHandler<DeleteAccountCommand>
    {
        private readonly IAppDbContext _context;

        public DeleteAccountHandler(IAppDbContext context)
        {
            _context = context;
        }

        public async Task Handle(DeleteAccountCommand request, CancellationToken cancellationToken)
        {
            var account = _context.Accounts.FirstOrDefault(a => a.Id == request.Id);

            if (account == null)
                throw new NotFoundException(request.Id.ToString());

            _context.Accounts.Remove(account);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
