using SharedLibrary.Entities;

namespace AuthService.Domain.Entities
{
    public class RefreshToken : EntityBase<Guid>
    {
        public Guid UserId { get; set; }
        public string Token { get; set; } = string.Empty;
        public DateTimeOffset ExpireAt { get; set; }
        public bool IsRevoked { get; set; }
    }
}
