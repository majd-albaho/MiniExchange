using SharedLibrary.Entities;

namespace AuthService.Domain.Entities
{
    public class AuthUser : EntityBase<Guid>
    {
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
    }
}
