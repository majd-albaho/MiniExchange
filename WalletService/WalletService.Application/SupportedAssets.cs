using WalletService.Domain.Enums;

namespace WalletService.Application
{
    /// <summary>
    /// Central catalog of the real, on-chain assets this sandbox actually supports.
    /// Everything else is a ledger-only demo token that cannot be withdrawn on-chain.
    /// </summary>
    public static class SupportedAssets
    {
        public const string Ethereum = "ETH";

        /// <summary>Real symbols that must never be created as demo tokens.</summary>
        public static readonly IReadOnlySet<string> ReservedRealSymbols =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase) { Ethereum };

        public static bool IsWithdrawable(bool isDemo, CryptoNetworkType networkType)
            => !isDemo && networkType == CryptoNetworkType.Ethereum;

        public static CryptoNetworkType NetworkFor(string symbol)
            => string.Equals(symbol, Ethereum, StringComparison.OrdinalIgnoreCase)
                ? CryptoNetworkType.Ethereum
                : CryptoNetworkType.None;
    }
}
