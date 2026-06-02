using Microsoft.EntityFrameworkCore;
using WalletService.Domain.Entities;
using WalletService.Infrastructure.Persistence;

namespace WalletService.Infrastructure.Repositories
{
    internal class WalletRepository
    {
        private readonly WalletDbContext _context;

        public WalletRepository(WalletDbContext context)
        {
            _context = context;
        }

        public Task<Wallet?> GetWalletByUserId(Guid userId)
        {
            var query = from wallet in _context.Wallets
                        from userWallet in _context.UserWallets
                        where userWallet.UserId == userId && userWallet.WalletId == wallet.Id
                        select wallet;

            return query.FirstOrDefaultAsync();
        }
    }
}
