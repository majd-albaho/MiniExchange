using AuthService.Application.Dto;
using AuthService.Application.Interfaces.Repositories;
using AuthService.Application.Interfaces.Services;
using AuthService.Domain.Entities;
using SharedLibrary.EventModel;

namespace AuthService.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IAuthUserRepository _userRepository;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IMessageBroker _messageBroker;

        public AuthService(IAuthUserRepository userRepository, IRefreshTokenRepository refreshTokenRepository,
            IJwtTokenService jwtTokenService, IPasswordHasher passwordHasher, IMessageBroker messageBroker) {
            _userRepository = userRepository;
            _refreshTokenRepository = refreshTokenRepository;
            _jwtTokenService = jwtTokenService;
            _passwordHasher = passwordHasher;
            _messageBroker = messageBroker;
        }

        public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default) {
            var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
            if (user == null)
                throw new UnauthorizedAccessException("Invalid email or password.");

            if (!user.IsActive)
                throw new UnauthorizedAccessException("Account is disabled.");

            if (!_passwordHasher.Verify(request.Password, user.PasswordHash))
                throw new UnauthorizedAccessException("Invalid email or password.");

            return await IssueTokensAsync(user, cancellationToken);
        }

        public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default) {
            var existing = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
            if (existing is not null)
                throw new InvalidOperationException("Email is already registered.");

            var user = new AuthUser {
                Id = Guid.NewGuid(),
                Email = request.Email,
                PasswordHash = _passwordHasher.Hash(request.Password),
                Role = request.Role,
                CreatedDate = DateTimeOffset.UtcNow,
                CreatedBy = request.Email
            };

            await _userRepository.AddAsync(user, cancellationToken);
            await _userRepository.SaveChangesAsync(cancellationToken);

            await _messageBroker.PublishAsync("wallet.user.registered", new UserRegisteredEvent {
                UserId = user.Id,
                Email = user.Email,
                CreatedDateTime = DateTimeOffset.UtcNow
            });

            return await IssueTokensAsync(user, cancellationToken);
        }

        public async Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default) {
            var stored = await _refreshTokenRepository.GetByTokenAsync(request.RefreshToken, cancellationToken);
            if (stored is null)
                throw new UnauthorizedAccessException("Invalid refresh token.");

            if (stored.IsRevoked)
                throw new UnauthorizedAccessException("Refresh token has been revoked.");

            if (stored.ExpireAt < DateTimeOffset.UtcNow)
                throw new UnauthorizedAccessException("Refresh token has expired.");

            var user = await _userRepository.GetByIdAsync(stored.UserId, cancellationToken);
            if (user is null)
                throw new UnauthorizedAccessException("User not found.");

            await _refreshTokenRepository.RevokeAsync(request.RefreshToken, cancellationToken);
            await _refreshTokenRepository.SaveChangesAsync(cancellationToken);

            return await IssueTokensAsync(user, cancellationToken);
        }

        public async Task LogoutAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default) {
            var stored = await _refreshTokenRepository.GetByTokenAsync(request.RefreshToken, cancellationToken);
            if (stored is null || stored.IsRevoked)
                return;

            await _refreshTokenRepository.RevokeAsync(request.RefreshToken, cancellationToken);
            await _refreshTokenRepository.SaveChangesAsync(cancellationToken);
        }

        private async Task<AuthResponse> IssueTokensAsync(AuthUser user, CancellationToken cancellationToken) {
            var accessToken = _jwtTokenService.GenerateAccessToken(user);
            var rawRefreshToken = _jwtTokenService.GenerateRefreshToken();
            var expiry = _jwtTokenService.GetRefreshTokenExpiry();

            var refreshToken = new RefreshToken {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Token = rawRefreshToken,
                ExpireAt = expiry,
                CreatedDate = DateTimeOffset.UtcNow,
                CreatedBy = user.Email
            };

            await _refreshTokenRepository.AddAsync(refreshToken, cancellationToken);
            await _refreshTokenRepository.SaveChangesAsync(cancellationToken);

            return new AuthResponse {
                AccessToken = accessToken,
                RefreshToken = rawRefreshToken,
                ExpiresAt = expiry
            };
        }
    }
}

