using AuthService.Domain.Entities;

namespace AuthService.Application.Interfaces.Services
{
    public interface IJwtTokenService
    {
        string GenerateAccessToken(AuthUser user);
        string GenerateRefreshToken();
        DateTimeOffset GetRefreshTokenExpiry();
    }
}
