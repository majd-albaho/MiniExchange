using AuthService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Infrastructure.Persistence
{
    public class AuthDbContext : DbContext
    {
        public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options) { }

        public DbSet<AuthUser> Users => Set<AuthUser>();
        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<AuthUser>(e =>
            {
                e.HasKey(x => x.Id);
                e.HasIndex(x => x.Email).IsUnique();
                e.Property(x => x.Email).IsRequired().HasMaxLength(256);
                e.Property(x => x.PasswordHash).IsRequired();
                e.Property(x => x.Role).IsRequired().HasMaxLength(50);
            });

            modelBuilder.Entity<RefreshToken>(e =>
            {
                e.HasKey(x => x.Id);
                e.HasIndex(x => x.Token).IsUnique();
                e.Property(x => x.Token).IsRequired().HasMaxLength(512);
                e.HasOne<AuthUser>()
                    .WithMany()
                    .HasForeignKey(x => x.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}

