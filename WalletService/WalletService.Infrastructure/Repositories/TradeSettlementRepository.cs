using Microsoft.EntityFrameworkCore;
using WalletService.Application.Interfaces.Repositories;
using WalletService.Application.Models;
using WalletService.Domain.Entities;
using WalletService.Domain.Enums;
using WalletService.Infrastructure.Persistence;

namespace WalletService.Infrastructure.Repositories
{
    public class TradeSettlementRepository : ITradeSettlementRepository
    {
        private readonly WalletDbContext _context;
        private readonly IWalletTransactionRepository _walletTransactionRepository;

        public TradeSettlementRepository(WalletDbContext context, IWalletTransactionRepository walletTransactionRepository)
        {
            _context = context;
            _walletTransactionRepository = walletTransactionRepository;
        }

        public async Task<bool> SettleTradeAsync(TradeSettlementCommand command, CancellationToken cancellationToken = default)
        {
            if (await _walletTransactionRepository.ExistsByReferenceAsync(command.TradeId, cancellationToken))
            {
                return true;
            }

            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

            var buyerQuote = await GetOrCreateAsync(command.BuyerWalletId, command.QuoteAssetId, command.ActorName, cancellationToken);
            var buyerBase = await GetOrCreateAsync(command.BuyerWalletId, command.BaseAssetId, command.ActorName, cancellationToken);
            var sellerBase = await GetOrCreateAsync(command.SellerWalletId, command.BaseAssetId, command.ActorName, cancellationToken);
            var sellerQuote = await GetOrCreateAsync(command.SellerWalletId, command.QuoteAssetId, command.ActorName, cancellationToken);

            if (buyerQuote.LockedAmount < command.QuoteAmount)
                throw new InvalidOperationException($"Buyer wallet {command.BuyerWalletId} does not have enough locked balance of asset {command.QuoteAssetId} to settle trade {command.TradeId}.");

            if (sellerBase.LockedAmount < command.Quantity)
                throw new InvalidOperationException($"Seller wallet {command.SellerWalletId} does not have enough locked balance of asset {command.BaseAssetId} to settle trade {command.TradeId}.");

            var now = DateTimeOffset.UtcNow;

            buyerQuote.LockedAmount -= command.QuoteAmount;
            buyerQuote.Amount -= command.QuoteAmount;
            buyerQuote.ModifiedBy = command.ActorName;
            buyerQuote.ModifiedDate = now;

            buyerBase.Amount += command.Quantity;
            buyerBase.ModifiedBy = command.ActorName;
            buyerBase.ModifiedDate = now;

            sellerBase.LockedAmount -= command.Quantity;
            sellerBase.Amount -= command.Quantity;
            sellerBase.ModifiedBy = command.ActorName;
            sellerBase.ModifiedDate = now;

            sellerQuote.Amount += command.QuoteAmount;
            sellerQuote.ModifiedBy = command.ActorName;
            sellerQuote.ModifiedDate = now;

            await _context.WalletTransactions.AddRangeAsync(
            [
                CreateLedgerEntry(command.BuyerWalletId, command.QuoteAssetId, WalletTransactionType.TradeDebit, command.QuoteAmount, buyerQuote.Amount, command, now),
                CreateLedgerEntry(command.BuyerWalletId, command.BaseAssetId, WalletTransactionType.TradeCredit, command.Quantity, buyerBase.Amount, command, now),
                CreateLedgerEntry(command.SellerWalletId, command.BaseAssetId, WalletTransactionType.TradeDebit, command.Quantity, sellerBase.Amount, command, now),
                CreateLedgerEntry(command.SellerWalletId, command.QuoteAssetId, WalletTransactionType.TradeCredit, command.QuoteAmount, sellerQuote.Amount, command, now)
            ], cancellationToken);

            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return true;
        }

        private async Task<UserWalletAsset> GetOrCreateAsync(long userWalletId, long assetId, string createdBy, CancellationToken cancellationToken)
        {
            var existing = await _context.UserWalletAssets
                .FirstOrDefaultAsync(a => a.UserWalletId == userWalletId && a.AssetId == assetId, cancellationToken);

            if (existing is not null)
            {
                return existing;
            }

            var created = new UserWalletAsset
            {
                Id = default,
                UserWalletId = userWalletId,
                AssetId = assetId,
                Amount = 0m,
                LockedAmount = 0m,
                CreatedBy = createdBy,
                CreatedDate = DateTimeOffset.UtcNow
            };

            await _context.UserWalletAssets.AddAsync(created, cancellationToken);
            return created;
        }

        private static WalletTransaction CreateLedgerEntry(
            long userWalletId, long assetId, WalletTransactionType type, decimal amount, decimal balanceAfter, TradeSettlementCommand command, DateTimeOffset now)
        {
            return new WalletTransaction
            {
                Id = default,
                UserWalletId = userWalletId,
                AssetId = assetId,
                Type = type,
                Amount = amount,
                BalanceAfter = balanceAfter,
                ReferenceId = command.TradeId,
                CreatedBy = command.ActorName,
                CreatedDate = now
            };
        }
    }
}
