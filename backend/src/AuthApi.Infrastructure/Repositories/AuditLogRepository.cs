using AuthApi.Domain.Entities;
using AuthApi.Domain.Interfaces.Repositories;
using AuthApi.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AuthApi.Infrastructure.Repositories;

public class AuditLogRepository(AppDbContext db) : IAuditLogRepository
{
    public async Task CreateAsync(AuditLog log)
    {
        db.AuditLogs.Add(log);
        await db.SaveChangesAsync();
    }

    public async Task<List<AuditLog>> GetAllAsync(int limit = 100, int skip = 0) =>
        await db.AuditLogs
            .OrderByDescending(l => l.CreatedAt)
            .Skip(skip)
            .Take(limit)
            .ToListAsync();

    public async Task<List<AuditLog>> GetByUserAsync(string userId, int limit = 50) =>
        await db.AuditLogs
            .Where(l => l.UserId == userId)
            .OrderByDescending(l => l.CreatedAt)
            .Take(limit)
            .ToListAsync();

    public async Task<List<AuditLog>> GetByPathAsync(string path, int limit = 50) =>
        await db.AuditLogs
            .Where(l => l.Path.Contains(path))
            .OrderByDescending(l => l.CreatedAt)
            .Take(limit)
            .ToListAsync();

    public async Task<long> CountAsync() =>
        await db.AuditLogs.LongCountAsync();
}
