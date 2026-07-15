using WalletService.Application.Models;
using WalletService.Domain.Entities;
using WalletService.Domain.Enums;

namespace WalletService.Application.Interfaces.Repositories
{
    public interface IWalletTransactionRepository
    {
        Task<WalletTransaction> RecordAsync(WalletTransaction transaction, CancellationToken cancellationToken = default);

        /// <summary>Newest-first page of a wallet's ledger, optionally filtered by entry type, asset name and date range.</summary>
        Task<WalletTransactionHistoryPage> GetPageByWalletAsync(
            long userWalletId,
            IReadOnlyCollection<WalletTransactionType>? types,
            string? assetName,
            DateTimeOffset? from,
            DateTimeOffset? to,
            int page,
            int pageSize,
            CancellationToken cancellationToken = default);

        /// <summary>Used to make trade settlement idempotent against redelivered trade events.</summary>
        Task<bool> ExistsByReferenceAsync(Guid referenceId, CancellationToken cancellationToken = default);

        /// <summary>Used to make on-chain deposit crediting idempotent against redelivered webhooks.</summary>
        Task<bool> ExistsByExternalReferenceAsync(string externalReference, CancellationToken cancellationToken = default);
    }
}
