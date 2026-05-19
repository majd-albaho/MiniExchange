using AuthService.Infrastructure.Services;

namespace AuthService.Tests.Services
{
    public class PasswordHasherTests
    {
        private readonly PasswordHasher _sut = new();

        [Fact]
        public void Hash_ReturnsNonEmptyString()
        {
            var hash = _sut.Hash("myPassword123");

            Assert.False(string.IsNullOrWhiteSpace(hash));
        }

        [Fact]
        public void Hash_SamePasswordProducesDifferentHashes()
        {
            var hash1 = _sut.Hash("myPassword123");
            var hash2 = _sut.Hash("myPassword123");

            Assert.NotEqual(hash1, hash2);
        }

        [Fact]
        public void Verify_CorrectPassword_ReturnsTrue()
        {
            var hash = _sut.Hash("myPassword123");

            var result = _sut.Verify("myPassword123", hash);

            Assert.True(result);
        }

        [Fact]
        public void Verify_WrongPassword_ReturnsFalse()
        {
            var hash = _sut.Hash("myPassword123");

            var result = _sut.Verify("wrongPassword", hash);

            Assert.False(result);
        }

        [Fact]
        public void Verify_TamperedHash_ReturnsFalse()
        {
            var result = _sut.Verify("myPassword123", "invalidsalt.invalidhash");

            Assert.False(result);
        }
    }
}
