using Microsoft.EntityFrameworkCore;
using WalletService.Application.Interfaces.Repositories;
using WalletService.Domain.Entities;
using WalletService.Infrastructure.Persistence;

namespace WalletService.Infrastructure.Repositories
{
    public class WalletTransactionRepository : IWalletTransactionRepository
    {
        private readonly WalletDbContext _context;

        public WalletTransactionRepository(WalletDbContext context)
        {
            _context = context;
        }

        public async Task<WalletTransaction> RecordAsync(WalletTransaction transaction, CancellationToken cancellationToken = default)
        {
            await _context.WalletTransactions.AddAsync(transaction, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return transaction;
        }

        public Task<bool> ExistsByReferenceAsync(Guid referenceId, CancellationToken cancellationToken = default)
        {
            return _context.WalletTransactions.AnyAsync(t => t.ReferenceId == referenceId, cancellationToken);
        }
    }
}
