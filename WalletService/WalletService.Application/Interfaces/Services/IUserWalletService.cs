using WalletService.Domain.Entities;

namespace WalletService.Application.Interfaces.Services
{
    public interface IUserWalletService
    {
        Task<UserWallet> GetUserWallet(Guid userId);
        Task<decimal> CheckBalance(Guid userId);
        Task<string> SendEtherium(Guid userId, string recipientAddress, decimal amount);
        Task<decimal> LockFund(Guid userId, decimal amount, CancellationToken cancellationToken = default);
        Task<decimal> UnlockFund(Guid userId, decimal amount, CancellationToken cancellationToken = default);


        Task<string> GetTransactionDetails(string transactionId);
    }
}
