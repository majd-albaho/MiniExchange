using Microsoft.EntityFrameworkCore;
using WalletService.Application.Interfaces.Repositories;
using WalletService.Domain.Entities;
using WalletService.Domain.Enums;
using WalletService.Infrastructure.Persistence;

namespace WalletService.Infrastructure.Repositories
{
    internal class UserWalletAddressRepository : IUserWalletAddressRepository
    {
        private readonly WalletDbContext _context;

        public UserWalletAddressRepository(WalletDbContext context)
        {
            _context = context;
        }

        public Task<UserWalletAddress?> GetByUserWalletId(long userWalletId, CryptoNetworkType cryptoNetworkType, CancellationToken cancellationToken = default)
        {
            return _context.UserWalletAddresses.AsNoTracking()
                .FirstOrDefaultAsync(w => w.UserWalletId == userWalletId && w.CryptoNetworkType == cryptoNetworkType, cancellationToken);
        }

        public async Task<UserWalletAddress> AddAsync(UserWalletAddress userWalletAddress, CancellationToken cancellationToken = default)
        {
            var res = await _context.UserWalletAddresses.AddAsync(userWalletAddress, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return res.Entity;
        }
    }
}
