using Microsoft.Extensions.Logging;
using WalletService.Application.Interfaces.Repositories;
using WalletService.Application.Interfaces.Services;
using WalletService.Application.Models;

namespace WalletService.Application.Services
{
    public class WalletSettlementService : IWalletSettlementService
    {
        private const string SystemActor = "TradeSettlement";

        private readonly IUserWalletService _userWalletService;
        private readonly ITradeSettlementRepository _tradeSettlementRepository;
        private readonly ILogger<WalletSettlementService> _logger;

        public WalletSettlementService(
            IUserWalletService userWalletService,
            ITradeSettlementRepository tradeSettlementRepository,
            ILogger<WalletSettlementService> logger)
        {
            _userWalletService = userWalletService;
            _tradeSettlementRepository = tradeSettlementRepository;
            _logger = logger;
        }

        public async Task<bool> SettleTradeAsync(TradeSettlementRequest request, CancellationToken cancellationToken = default)
        {
            if (request.Quantity <= 0m)
                throw new ArgumentException("Quantity must be greater than zero.", nameof(request));

            if (request.Price <= 0m)
                throw new ArgumentException("Price must be greater than zero.", nameof(request));

            var buyerWallet = await _userWalletService.GetUserWallet(request.BuyerUserId, cancellationToken);
            var sellerWallet = await _userWalletService.GetUserWallet(request.SellerUserId, cancellationToken);

            var settled = await _tradeSettlementRepository.SettleTradeAsync(new TradeSettlementCommand
            {
                TradeId = request.TradeId,
                BuyerWalletId = buyerWallet.Id,
                SellerWalletId = sellerWallet.Id,
                BaseAssetId = request.BaseAssetId,
                QuoteAssetId = request.QuoteAssetId,
                Quantity = request.Quantity,
                QuoteAmount = request.Quantity * request.Price,
                ActorName = SystemActor
            }, cancellationToken);

            _logger.LogInformation(
                "Settled trade {TradeId}: {Quantity} @ {Price} between buyer wallet {BuyerWalletId} and seller wallet {SellerWalletId}",
                request.TradeId, request.Quantity, request.Price, buyerWallet.Id, sellerWallet.Id);

            return settled;
        }
    }
}
