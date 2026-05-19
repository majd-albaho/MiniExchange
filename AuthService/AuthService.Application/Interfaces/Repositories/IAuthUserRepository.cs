using AuthService.Domain.Entities;

namespace AuthService.Application.Interfaces.Repositories
{
    public interface IAuthUserRepository
    {
        Task<AuthUser?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
        Task<AuthUser?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task AddAsync(AuthUser user, CancellationToken cancellationToken = default);
        Task SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
