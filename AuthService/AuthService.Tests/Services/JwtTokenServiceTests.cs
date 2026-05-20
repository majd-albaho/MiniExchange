using System.IdentityModel.Tokens.Jwt;
using AuthService.Domain.Entities;
using AuthService.Infrastructure.Services;
using Microsoft.Extensions.Configuration;

namespace AuthService.Tests.Services
{
    public class JwtTokenServiceTests
    {
        private readonly JwtTokenService _sut;

        private static readonly AuthUser TestUser = new()
        {
            Id = Guid.NewGuid(),
            Email = "user@test.com",
            Role = "User",
            CreatedDate = DateTimeOffset.UtcNow,
            CreatedBy = "test"
        };

        public JwtTokenServiceTests()
        {
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["JwtSettings:SecretKey"]              = "super-secret-key-for-testing-minimum-32-chars!!",
                    ["JwtSettings:Issuer"]                 = "TestIssuer",
                    ["JwtSettings:Audience"]               = "TestAudience",
                    ["JwtSettings:AccessTokenExpiryMinutes"] = "60",
                    ["JwtSettings:RefreshTokenExpiryDays"] = "7"
                })
                .Build();

            _sut = new JwtTokenService(config);
        }

        [Fact]
        public void GenerateAccessToken_ReturnsValidJwtWithClaims()
        {
            var token = _sut.GenerateAccessToken(TestUser);

            Assert.False(string.IsNullOrWhiteSpace(token));

            var handler = new JwtSecurityTokenHandler();
            var parsed = handler.ReadJwtToken(token);

            Assert.Equal("TestIssuer", parsed.Issuer);
            Assert.Contains(parsed.Audiences, a => a == "TestAudience");
            Assert.Contains(parsed.Claims, c => c.Type == JwtRegisteredClaimNames.Email && c.Value == TestUser.Email);
            Assert.Contains(parsed.Claims, c => c.Type == JwtRegisteredClaimNames.Sub && c.Value == TestUser.Id.ToString());
        }

        [Fact]
        public void GenerateAccessToken_ExpiryIsWithinExpectedWindow()
        {
            var before = DateTime.UtcNow.AddMinutes(59);
            var token = _sut.GenerateAccessToken(TestUser);
            var after = DateTime.UtcNow.AddMinutes(61);

            var parsed = new JwtSecurityTokenHandler().ReadJwtToken(token);

            Assert.True(parsed.ValidTo >= before && parsed.ValidTo <= after);
        }

        [Fact]
        public void GenerateRefreshToken_ReturnsDifferentTokensEachCall()
        {
            var token1 = _sut.GenerateRefreshToken();
            var token2 = _sut.GenerateRefreshToken();

            Assert.False(string.IsNullOrWhiteSpace(token1));
            Assert.NotEqual(token1, token2);
        }

        [Fact]
        public void GenerateRefreshToken_IsValidBase64()
        {
            var token = _sut.GenerateRefreshToken();

            var bytes = Convert.FromBase64String(token);
            Assert.Equal(64, bytes.Length);
        }

        [Fact]
        public void GetRefreshTokenExpiry_IsApproximatelySevenDaysFromNow()
        {
            var before = DateTimeOffset.UtcNow.AddDays(6).AddHours(23);
            var expiry = _sut.GetRefreshTokenExpiry();
            var after = DateTimeOffset.UtcNow.AddDays(7).AddMinutes(1);

            Assert.True(expiry >= before && expiry <= after);
        }
    }
}
