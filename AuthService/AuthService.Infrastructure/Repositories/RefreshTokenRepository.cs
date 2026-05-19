using AuthService.Application.Interfaces.Repositories;
using AuthService.Domain.Entities;
using AuthService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Infrastructure.Repositories
{
    public class RefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly AuthDbContext _context;

        public RefreshTokenRepository(AuthDbContext context)
        {
            _context = context;
        }

        public async Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default)
            => await _context.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == token, cancellationToken);

        public async Task AddAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default)
            => await _context.RefreshTokens.AddAsync(refreshToken, cancellationToken);

        public async Task RevokeAsync(string token, CancellationToken cancellationToken = default)
        {
            var stored = await _context.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == token, cancellationToken);
            if (stored is not null)
                stored.IsRevoked = true;
        }

        public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
            => await _context.SaveChangesAsync(cancellationToken);
    }
}
