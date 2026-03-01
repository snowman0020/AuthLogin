using AuthApi.Domain.Entities;
using AuthApi.Domain.Interfaces.Repositories;
using AuthApi.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AuthApi.Infrastructure.Repositories;

public class ApiKeyRepository(AppDbContext db) : IApiKeyRepository
{
    public async Task<ApiKey?> GetByKeyAsync(string key) =>
        await db.ApiKeys.FirstOrDefaultAsync(ak => ak.Key == key);

    public async Task<ApiKey?> GetByIdAndUserAsync(string id, string userId) =>
        await db.ApiKeys.FirstOrDefaultAsync(ak => ak.Id == id && ak.UserId == userId);

    public async Task<List<ApiKey>> GetAllByUserAsync(string userId) =>
        await db.ApiKeys
            .Where(ak => ak.UserId == userId)
            .OrderByDescending(ak => ak.CreatedAt)
            .ToListAsync();

    public async Task CreateAsync(ApiKey apiKey)
    {
        db.ApiKeys.Add(apiKey);
        await db.SaveChangesAsync();
    }

    public async Task DeactivateAsync(string id)
    {
        await db.ApiKeys
            .Where(ak => ak.Id == id)
            .ExecuteUpdateAsync(s => s.SetProperty(ak => ak.IsActive, false));
    }

    public async Task UpdateLastUsedAsync(string id)
    {
        await db.ApiKeys
            .Where(ak => ak.Id == id)
            .ExecuteUpdateAsync(s => s.SetProperty(ak => ak.LastUsedAt, DateTime.UtcNow));
    }
}
