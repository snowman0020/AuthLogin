using AuthApi.Domain.Entities;
using AuthApi.Domain.Interfaces.Repositories;
using AuthApi.Infrastructure.Data;
using MongoDB.Driver;

namespace AuthApi.Infrastructure.Repositories;

public class AuditLogRepository(MongoDbContext db) : IAuditLogRepository
{
    public async Task CreateAsync(AuditLog log) =>
        await db.AuditLogs.InsertOneAsync(log);

    public async Task<List<AuditLog>> GetAllAsync(int limit = 100, int skip = 0) =>
        await db.AuditLogs
            .Find(_ => true)
            .SortByDescending(l => l.CreatedAt)
            .Skip(skip)
            .Limit(limit)
            .ToListAsync();

    public async Task<List<AuditLog>> GetByUserAsync(string userId, int limit = 50) =>
        await db.AuditLogs
            .Find(l => l.UserId == userId)
            .SortByDescending(l => l.CreatedAt)
            .Limit(limit)
            .ToListAsync();

    public async Task<List<AuditLog>> GetByPathAsync(string path, int limit = 50) =>
        await db.AuditLogs
            .Find(l => l.Path.Contains(path))
            .SortByDescending(l => l.CreatedAt)
            .Limit(limit)
            .ToListAsync();

    public async Task<long> CountAsync() =>
        await db.AuditLogs.CountDocumentsAsync(_ => true);
}
