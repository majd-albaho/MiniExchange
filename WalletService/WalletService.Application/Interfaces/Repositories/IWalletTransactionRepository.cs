using WalletService.Domain.Entities;

namespace WalletService.Application.Interfaces.Repositories
{
    public interface IWalletTransactionRepository
    {
        Task<WalletTransaction> RecordAsync(WalletTransaction transaction, CancellationToken cancellationToken = default);

        /// <summary>Used to make trade settlement idempotent against redelivered trade events.</summary>
        Task<bool> ExistsByReferenceAsync(Guid referenceId, CancellationToken cancellationToken = default);

        /// <summary>Used to make on-chain deposit crediting idempotent against redelivered webhooks.</summary>
        Task<bool> ExistsByExternalReferenceAsync(string externalReference, CancellationToken cancellationToken = default);
    }
}
