using WalletService.Application.Dto;
using WalletService.Application.Models;

namespace WalletService.Application.Interfaces.Services
{
    public interface IWalletTransactionHistoryService
    {
        Task<WalletTransactionHistoryResponseDto> GetHistoryAsync(Guid userId, WalletTransactionHistoryQuery query, CancellationToken cancellationToken = default);
    }
}
