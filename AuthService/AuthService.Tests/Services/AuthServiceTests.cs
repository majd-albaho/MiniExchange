using AuthService.Application.Dto;
using AuthService.Application.Interfaces.Repositories;
using AuthService.Application.Interfaces.Services;
using AuthService.Domain.Entities;
using Moq;
using SharedLibrary.EventDriven;

namespace AuthService.Tests.Services
{
    public class AuthServiceTests
    {
        private readonly Mock<IAuthUserRepository> _userRepoMock = new();
        private readonly Mock<IRefreshTokenRepository> _refreshTokenRepoMock = new();
        private readonly Mock<IJwtTokenService> _jwtTokenServiceMock = new();
        private readonly Mock<IPasswordHasher> _passwordHasherMock = new();
        private readonly Mock<IMessageBroker> _messageBrokerMock = new();
        private readonly Application.Services.AuthService _sut;

        private static readonly DateTimeOffset TokenExpiry = DateTimeOffset.UtcNow.AddDays(7);

        public AuthServiceTests() {
            _sut = new Application.Services.AuthService(
                _userRepoMock.Object,
                _refreshTokenRepoMock.Object,
                _jwtTokenServiceMock.Object,
                _passwordHasherMock.Object,
                _messageBrokerMock.Object);

            // Default token service setup
            _jwtTokenServiceMock.Setup(x => x.GenerateAccessToken(It.IsAny<AuthUser>())).Returns("access-token");
            _jwtTokenServiceMock.Setup(x => x.GenerateRefreshToken()).Returns("refresh-token");
            _jwtTokenServiceMock.Setup(x => x.GetRefreshTokenExpiry()).Returns(TokenExpiry);
        }

        // ── Login ────────────────────────────────────────────────────────────────

        [Fact]
        public async Task Login_ValidCredentials_ReturnsAuthResponse() {
            // Arrange
            var user = BuildUser();
            _userRepoMock.Setup(x => x.GetByEmailAsync(user.Email, default)).ReturnsAsync(user);
            _passwordHasherMock.Setup(x => x.Verify("password", user.PasswordHash)).Returns(true);

            // Act
            var result = await _sut.LoginAsync(new LoginRequest { Email = user.Email, Password = "password" });

            // Assert
            Assert.Equal("access-token", result.AccessToken);
            Assert.Equal("refresh-token", result.RefreshToken);
            Assert.Equal(TokenExpiry, result.ExpiresAt);
        }

        [Fact]
        public async Task Login_UserNotFound_ThrowsUnauthorizedAccessException() {
            // Arrange
            _userRepoMock.Setup(x => x.GetByEmailAsync(It.IsAny<string>(), default)).ReturnsAsync((AuthUser?)null);

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                _sut.LoginAsync(new LoginRequest { Email = "nope@test.com", Password = "x" }));
        }

        [Fact]
        public async Task Login_WrongPassword_ThrowsUnauthorizedAccessException() {
            // Arrange
            var user = BuildUser();
            _userRepoMock.Setup(x => x.GetByEmailAsync(user.Email, default)).ReturnsAsync(user);
            _passwordHasherMock.Setup(x => x.Verify(It.IsAny<string>(), user.PasswordHash)).Returns(false);

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                _sut.LoginAsync(new LoginRequest { Email = user.Email, Password = "wrong" }));
        }

        [Fact]
        public async Task Login_InactiveUser_ThrowsUnauthorizedAccessException() {
            // Arrange
            var user = BuildUser(isActive: false);
            _userRepoMock.Setup(x => x.GetByEmailAsync(user.Email, default)).ReturnsAsync(user);

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                _sut.LoginAsync(new LoginRequest { Email = user.Email, Password = "password" }));
        }

        // ── Register ─────────────────────────────────────────────────────────────

        [Fact]
        public async Task Register_NewEmail_ReturnsAuthResponse() {
            // Arrange
            _userRepoMock.Setup(x => x.GetByEmailAsync("new@test.com", default)).ReturnsAsync((AuthUser?)null);
            _passwordHasherMock.Setup(x => x.Hash("password")).Returns("hashed-password");

            // Act
            var result = await _sut.RegisterAsync(new RegisterRequest {
                Email = "new@test.com",
                Password = "password",
                Role = "User"
            });

            // Assert
            Assert.Equal("access-token", result.AccessToken);
            _userRepoMock.Verify(x => x.AddAsync(It.Is<AuthUser>(u => u.Email == "new@test.com"), default), Times.Once);
            _userRepoMock.Verify(x => x.SaveChangesAsync(default), Times.Once);
        }

        [Fact]
        public async Task Register_DuplicateEmail_ThrowsInvalidOperationException() {
            // Arrange
            var existing = BuildUser();
            _userRepoMock.Setup(x => x.GetByEmailAsync(existing.Email, default)).ReturnsAsync(existing);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _sut.RegisterAsync(new RegisterRequest { Email = existing.Email, Password = "password" }));
        }

        // ── RefreshToken ─────────────────────────────────────────────────────────

        [Fact]
        public async Task RefreshToken_ValidToken_ReturnsNewAuthResponse() {
            // Arrange
            var user = BuildUser();
            var stored = BuildRefreshToken(user.Id);
            _refreshTokenRepoMock.Setup(x => x.GetByTokenAsync("old-token", default)).ReturnsAsync(stored);
            _userRepoMock.Setup(x => x.GetByIdAsync(user.Id, default)).ReturnsAsync(user);

            // Act
            var result = await _sut.RefreshTokenAsync(new RefreshTokenRequest { RefreshToken = "old-token" });

            // Assert
            Assert.Equal("access-token", result.AccessToken);
            _refreshTokenRepoMock.Verify(x => x.RevokeAsync("old-token", default), Times.Once);
        }

        [Fact]
        public async Task RefreshToken_TokenNotFound_ThrowsUnauthorizedAccessException() {
            // Arrange
            _refreshTokenRepoMock.Setup(x => x.GetByTokenAsync(It.IsAny<string>(), default)).ReturnsAsync((RefreshToken?)null);

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                _sut.RefreshTokenAsync(new RefreshTokenRequest { RefreshToken = "ghost" }));
        }

        [Fact]
        public async Task RefreshToken_RevokedToken_ThrowsUnauthorizedAccessException() {
            // Arrange
            var user = BuildUser();
            var stored = BuildRefreshToken(user.Id, isRevoked: true);
            _refreshTokenRepoMock.Setup(x => x.GetByTokenAsync("revoked-token", default)).ReturnsAsync(stored);

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                _sut.RefreshTokenAsync(new RefreshTokenRequest { RefreshToken = "revoked-token" }));
        }

        [Fact]
        public async Task RefreshToken_ExpiredToken_ThrowsUnauthorizedAccessException() {
            // Arrange
            var user = BuildUser();
            var stored = BuildRefreshToken(user.Id, expireAt: DateTimeOffset.UtcNow.AddDays(-1));
            _refreshTokenRepoMock.Setup(x => x.GetByTokenAsync("expired-token", default)).ReturnsAsync(stored);

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                _sut.RefreshTokenAsync(new RefreshTokenRequest { RefreshToken = "expired-token" }));
        }

        // ── Logout ───────────────────────────────────────────────────────────────

        [Fact]
        public async Task Logout_ValidToken_RevokesToken() {
            // Arrange
            var user = BuildUser();
            var stored = BuildRefreshToken(user.Id);
            _refreshTokenRepoMock.Setup(x => x.GetByTokenAsync("valid-token", default)).ReturnsAsync(stored);

            // Act
            await _sut.LogoutAsync(new RefreshTokenRequest { RefreshToken = "valid-token" });

            // Assert
            _refreshTokenRepoMock.Verify(x => x.RevokeAsync("valid-token", default), Times.Once);
            _refreshTokenRepoMock.Verify(x => x.SaveChangesAsync(default), Times.Once);
        }

        [Fact]
        public async Task Logout_AlreadyRevokedToken_DoesNotCallRevokeAgain() {
            // Arrange
            var user = BuildUser();
            var stored = BuildRefreshToken(user.Id, isRevoked: true);
            _refreshTokenRepoMock.Setup(x => x.GetByTokenAsync("revoked-token", default)).ReturnsAsync(stored);

            // Act
            await _sut.LogoutAsync(new RefreshTokenRequest { RefreshToken = "revoked-token" });

            // Assert
            _refreshTokenRepoMock.Verify(x => x.RevokeAsync(It.IsAny<string>(), default), Times.Never);
        }

        // ── Helpers ──────────────────────────────────────────────────────────────

        private static AuthUser BuildUser(bool isActive = true) => new() {
            Id = Guid.NewGuid(),
            Email = "user@test.com",
            PasswordHash = "hashed-password",
            Role = "User",
            IsActive = isActive,
            CreatedDate = DateTimeOffset.UtcNow,
            CreatedBy = "test"
        };

        private static RefreshToken BuildRefreshToken(
            Guid userId,
            bool isRevoked = false,
            DateTimeOffset? expireAt = null) => new() {
                Id = Guid.NewGuid(),
                UserId = userId,
                Token = "valid-token",
                IsRevoked = isRevoked,
                ExpireAt = expireAt ?? DateTimeOffset.UtcNow.AddDays(7),
                CreatedDate = DateTimeOffset.UtcNow,
                CreatedBy = "test"
            };
    }
}
