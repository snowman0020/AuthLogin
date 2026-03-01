using AuthApi.Domain.Entities;
using AuthApi.Domain.Interfaces.Repositories;
using AuthApi.Infrastructure.Data;
using MongoDB.Driver;

namespace AuthApi.Infrastructure.Repositories;

public class ApiKeyRepository(MongoDbContext db) : IApiKeyRepository
{
    public async Task<ApiKey?> GetByKeyAsync(string key) =>
        await db.ApiKeys.Find(ak => ak.Key == key).FirstOrDefaultAsync();

    public async Task<ApiKey?> GetByIdAndUserAsync(string id, string userId) =>
        await db.ApiKeys.Find(ak => ak.Id == id && ak.UserId == userId).FirstOrDefaultAsync();

    public async Task<List<ApiKey>> GetAllByUserAsync(string userId) =>
        await db.ApiKeys
            .Find(ak => ak.UserId == userId)
            .SortByDescending(ak => ak.CreatedAt)
            .ToListAsync();

    public async Task CreateAsync(ApiKey apiKey) =>
        await db.ApiKeys.InsertOneAsync(apiKey);

    public async Task DeactivateAsync(string id)
    {
        var update = Builders<ApiKey>.Update.Set(ak => ak.IsActive, false);
        await db.ApiKeys.UpdateOneAsync(ak => ak.Id == id, update);
    }

    public async Task UpdateLastUsedAsync(string id)
    {
        var update = Builders<ApiKey>.Update.Set(ak => ak.LastUsedAt, DateTime.UtcNow);
        await db.ApiKeys.UpdateOneAsync(ak => ak.Id == id, update);
    }
}
