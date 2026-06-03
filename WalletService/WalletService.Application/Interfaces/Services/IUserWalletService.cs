using WalletService.Application.Dto;
using WalletService.Domain.Entities;

namespace WalletService.Application.Interfaces.Services
{
    public interface IUserWalletService
    {
        Task<UserWalletDto> GetUserWallet(Guid userId);
        Task<decimal> CheckEthereumBalance(Guid userId);
        Task<string> SendEthereum(Guid userId, string recipientAddress, decimal amount);
        Task LockFund(Guid userId, long assetId, decimal amount, CancellationToken cancellationToken = default);
        Task UnlockFund(Guid userId, long assetId, decimal amount, CancellationToken cancellationToken = default);


        Task<string> GetTransactionDetails(string transactionId);
    }
}
