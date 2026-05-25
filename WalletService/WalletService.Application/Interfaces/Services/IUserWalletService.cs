namespace WalletService.Application.Interfaces.Services
{
    public interface IUserWalletService
    {
        Task<string> GetUserWallet(string userId);
    }
}
