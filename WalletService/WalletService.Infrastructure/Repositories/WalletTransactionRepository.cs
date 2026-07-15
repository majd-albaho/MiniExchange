using Microsoft.EntityFrameworkCore;
using WalletService.Application.Interfaces.Repositories;
using WalletService.Application.Models;
using WalletService.Domain.Entities;
using WalletService.Domain.Enums;
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

        public async Task<WalletTransactionHistoryPage> GetPageByWalletAsync(
            long userWalletId,
            IReadOnlyCollection<WalletTransactionType>? types,
            string? assetName,
            DateTimeOffset? from,
            DateTimeOffset? to,
            int page,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            var query =
                from tx in _context.WalletTransactions
                join asset in _context.Assets on tx.AssetId equals asset.Id
                where tx.UserWalletId == userWalletId
                select new { tx, asset.AssetName };

            if (types is { Count: > 0 })
            {
                query = query.Where(x => types.Contains(x.tx.Type));
            }

            if (assetName is not null)
            {
                query = query.Where(x => x.AssetName == assetName);
            }

            if (from is not null)
            {
                query = query.Where(x => x.tx.CreatedDate >= from);
            }

            if (to is not null)
            {
                query = query.Where(x => x.tx.CreatedDate <= to);
            }

            var total = await query.CountAsync(cancellationToken);

            var items = await query
                .OrderByDescending(x => x.tx.CreatedDate)
                .ThenByDescending(x => x.tx.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new WalletTransactionHistoryEntry
                {
                    Id = x.tx.Id,
                    AssetName = x.AssetName,
                    Type = x.tx.Type,
                    Amount = x.tx.Amount,
                    BalanceAfter = x.tx.BalanceAfter,
                    ReferenceId = x.tx.ReferenceId,
                    ExternalReference = x.tx.ExternalReference,
                    CreatedDate = x.tx.CreatedDate,
                })
                .ToListAsync(cancellationToken);

            return new WalletTransactionHistoryPage { Items = items, Total = total };
        }

        public Task<bool> ExistsByReferenceAsync(Guid referenceId, CancellationToken cancellationToken = default)
        {
            return _context.WalletTransactions.AnyAsync(t => t.ReferenceId == referenceId, cancellationToken);
        }

        public Task<bool> ExistsByExternalReferenceAsync(string externalReference, CancellationToken cancellationToken = default)
        {
            return _context.WalletTransactions.AnyAsync(t => t.ExternalReference == externalReference, cancellationToken);
        }
    }
}
