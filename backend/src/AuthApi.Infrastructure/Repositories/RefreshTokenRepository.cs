using AuthApi.Domain.Entities;
using AuthApi.Domain.Interfaces.Repositories;
using AuthApi.Infrastructure.Data;
using MongoDB.Driver;

namespace AuthApi.Infrastructure.Repositories;

public class RefreshTokenRepository(MongoDbContext db) : IRefreshTokenRepository
{
    public async Task<RefreshToken?> GetByTokenAsync(string token) =>
        await db.RefreshTokens.Find(rt => rt.Token == token).FirstOrDefaultAsync();

    public async Task CreateAsync(RefreshToken token) =>
        await db.RefreshTokens.InsertOneAsync(token);

    public async Task RevokeAsync(string tokenId, string reason)
    {
        var update = Builders<RefreshToken>.Update
            .Set(rt => rt.IsRevoked, true)
            .Set(rt => rt.RevokedReason, reason);

        await db.RefreshTokens.UpdateOneAsync(rt => rt.Id == tokenId, update);
    }

    public async Task RevokeAllByUserAsync(string userId, string reason)
    {
        var update = Builders<RefreshToken>.Update
            .Set(rt => rt.IsRevoked, true)
            .Set(rt => rt.RevokedReason, reason);

        await db.RefreshTokens.UpdateManyAsync(
            rt => rt.UserId == userId && !rt.IsRevoked,
            update);
    }
}
