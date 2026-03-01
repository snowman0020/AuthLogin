using AuthApi.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuthApi.Infrastructure.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<ApiKey> ApiKeys => Set<ApiKey>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder model)
    {
        model.Entity<User>(e =>
        {
            e.HasKey(u => u.Id);
            e.HasIndex(u => u.Email).IsUnique();
            e.HasIndex(u => u.Username).IsUnique();
        });

        model.Entity<RefreshToken>(e =>
        {
            e.HasKey(rt => rt.Id);
            e.HasIndex(rt => rt.Token).IsUnique();
            e.HasIndex(rt => rt.UserId);
            e.Ignore(rt => rt.IsExpired);
            e.Ignore(rt => rt.IsValid);
        });

        model.Entity<ApiKey>(e =>
        {
            e.HasKey(ak => ak.Id);
            e.HasIndex(ak => ak.Key).IsUnique();
            e.HasIndex(ak => ak.UserId);
            e.Ignore(ak => ak.IsExpired);
            e.Ignore(ak => ak.IsValid);
        });

        model.Entity<AuditLog>(e =>
        {
            e.HasKey(l => l.Id);
            e.HasIndex(l => l.CreatedAt);
            e.HasIndex(l => l.UserId);
            e.HasIndex(l => l.Path);
        });
    }
}
