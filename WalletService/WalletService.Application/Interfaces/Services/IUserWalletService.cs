using Nethereum.Hex.HexTypes;

namespace WalletService.Application.Interfaces.Services
{
    public interface IUserWalletService
    {
        Task<string> GetUserWallet(Guid userId);
        Task<HexBigInteger> CheckBalance(Guid userId);
    }
}
