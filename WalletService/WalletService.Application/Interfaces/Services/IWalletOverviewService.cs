using WalletService.Application.Dto;

namespace WalletService.Application.Interfaces.Services
{
    public interface IWalletOverviewService
    {
        /// <summary>Builds the wallet page view: live on-chain ETH plus all ledger (incl. demo) balances.</summary>
        Task<WalletOverviewDto> GetOverviewAsync(Guid userId, CancellationToken cancellationToken = default);

        /// <summary>Returns deposit details for a symbol. Real assets expose an on-chain address; demo tokens do not.</summary>
        Task<ReceiveInfoDto> GetReceiveInfoAsync(Guid userId, string symbol, string network, CancellationToken cancellationToken = default);
    }
}
