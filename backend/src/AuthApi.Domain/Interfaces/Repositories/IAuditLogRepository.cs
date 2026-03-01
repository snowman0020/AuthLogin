using AuthApi.Domain.Entities;

namespace AuthApi.Domain.Interfaces.Repositories;

public interface IAuditLogRepository
{
    Task CreateAsync(AuditLog log);
    Task<List<AuditLog>> GetAllAsync(int limit = 100, int skip = 0);
    Task<List<AuditLog>> GetByUserAsync(string userId, int limit = 50);
    Task<List<AuditLog>> GetByPathAsync(string path, int limit = 50);
    Task<long> CountAsync();
}
