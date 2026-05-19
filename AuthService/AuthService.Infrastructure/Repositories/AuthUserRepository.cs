using AuthService.Application.Interfaces.Repositories;
using AuthService.Domain.Entities;
using AuthService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Infrastructure.Repositories
{
    public class AuthUserRepository : IAuthUserRepository
    {
        private readonly AuthDbContext _context;

        public AuthUserRepository(AuthDbContext context)
        {
            _context = context;
        }

        public async Task<AuthUser?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
            => await _context.Users.FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

        public async Task<AuthUser?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => await _context.Users.FindAsync([id], cancellationToken);

        public async Task AddAsync(AuthUser user, CancellationToken cancellationToken = default)
            => await _context.Users.AddAsync(user, cancellationToken);

        public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
            => await _context.SaveChangesAsync(cancellationToken);
    }
}

