using WalletService.Application.Dto;
using WalletService.Application.Interfaces.Repositories;
using WalletService.Application.Interfaces.Services;
using WalletService.Application.Models;
using WalletService.Domain.Enums;

namespace WalletService.Application.Services
{
    public class WalletTransactionHistoryService : IWalletTransactionHistoryService
    {
        private const string QuoteSymbol = "USDT";

        // Frontend vocabulary → ledger entry types. TradeCredit is an asset received in a
        // trade ("buy"), TradeDebit an asset paid in a trade ("sell").
        private static readonly Dictionary<string, WalletTransactionType[]> TypeFilters = new(StringComparer.OrdinalIgnoreCase)
        {
            ["receive"] = [WalletTransactionType.Credit, WalletTransactionType.Deposit],
            ["send"] = [WalletTransactionType.Withdrawal],
            ["buy"] = [WalletTransactionType.TradeCredit],
            ["sell"] = [WalletTransactionType.TradeDebit],
        };

        private readonly IUserWalletService _userWalletService;
        private readonly IWalletTransactionRepository _walletTransactionRepository;

        public WalletTransactionHistoryService(
            IUserWalletService userWalletService,
            IWalletTransactionRepository walletTransactionRepository)
        {
            _userWalletService = userWalletService;
            _walletTransactionRepository = walletTransactionRepository;
        }

        public async Task<WalletTransactionHistoryResponseDto> GetHistoryAsync(Guid userId, WalletTransactionHistoryQuery query, CancellationToken cancellationToken = default)
        {
            var page = Math.Max(1, query.Page);
            var pageSize = Math.Clamp(query.PageSize, 1, 100);

            // Every ledger entry is final, so any other requested status can't match anything.
            if (!string.IsNullOrWhiteSpace(query.Status)
                && !query.Status.Equals("all", StringComparison.OrdinalIgnoreCase)
                && !query.Status.Equals("completed", StringComparison.OrdinalIgnoreCase))
            {
                return new WalletTransactionHistoryResponseDto { Page = page, PageSize = pageSize };
            }

            WalletTransactionType[]? types = null;
            if (!string.IsNullOrWhiteSpace(query.Type) && !query.Type.Equals("all", StringComparison.OrdinalIgnoreCase))
            {
                if (!TypeFilters.TryGetValue(query.Type, out types))
                {
                    return new WalletTransactionHistoryResponseDto { Page = page, PageSize = pageSize };
                }
            }

            var wallet = await _userWalletService.GetUserWallet(userId, cancellationToken);

            var result = await _walletTransactionRepository.GetPageByWalletAsync(
                wallet.Id,
                types,
                string.IsNullOrWhiteSpace(query.Symbol) ? null : query.Symbol.Trim().ToUpperInvariant(),
                query.StartDate,
                query.EndDate,
                page,
                pageSize,
                cancellationToken);

            return new WalletTransactionHistoryResponseDto
            {
                Items = result.Items.Select(Map).ToList(),
                Total = result.Total,
                Page = page,
                PageSize = pageSize,
            };
        }

        private static WalletTransactionHistoryItemDto Map(WalletTransactionHistoryEntry entry)
        {
            return new WalletTransactionHistoryItemDto
            {
                Id = entry.Id.ToString(),
                Type = MapType(entry.Type),
                Status = "completed",
                Symbol = entry.AssetName,
                Amount = entry.Amount,
                AmountUSDT = entry.AssetName == QuoteSymbol ? entry.Amount : 0m,
                Fee = 0m,
                FeeSymbol = entry.AssetName,
                TxHash = entry.ExternalReference ?? entry.ReferenceId?.ToString(),
                Network = null,
                CreatedAt = entry.CreatedDate,
                UpdatedAt = entry.CreatedDate,
            };
        }

        private static string MapType(WalletTransactionType type)
        {
            return type switch
            {
                WalletTransactionType.Credit => "receive",
                WalletTransactionType.Deposit => "receive",
                WalletTransactionType.Withdrawal => "send",
                WalletTransactionType.TradeCredit => "buy",
                WalletTransactionType.TradeDebit => "sell",
                _ => "receive",
            };
        }
    }
}
