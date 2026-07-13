using SharedLibrary.Entities;
using WalletService.Domain.Enums;

namespace WalletService.Domain.Entities
{
    public class Asset : EntityBase<long>
    {
        public required string AssetName { get; set; }
        public CryptoNetworkType CryptoNetworkType { get; set; }

        /// <summary>
        /// True for test-only tokens added via the "add demo token" flow. Demo balances exist
        /// only in the ledger and have no on-chain backing, so they can never be withdrawn/sent.
        /// </summary>
        public bool IsDemo { get; set; }
    }
}
