using AuthApi.Domain.Entities;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace AuthApi.Infrastructure.Data;

public class MongoDbContext
{
    private readonly IMongoDatabase _db;

    public MongoDbContext(IConfiguration config)
    {
        var uri = config["MongoDB:Uri"]
            ?? throw new InvalidOperationException("MongoDB:Uri is not configured");
        var dbName = config["MongoDB:DatabaseName"] ?? "authdb";

        var client = new MongoClient(uri);
        _db = client.GetDatabase(dbName);

        EnsureIndexes();
    }

    public IMongoCollection<User> Users => _db.GetCollection<User>("users");
    public IMongoCollection<RefreshToken> RefreshTokens => _db.GetCollection<RefreshToken>("refreshTokens");
    public IMongoCollection<ApiKey> ApiKeys => _db.GetCollection<ApiKey>("apiKeys");

    private void EnsureIndexes()
    {
        Users.Indexes.CreateMany([
            new CreateIndexModel<User>(
                Builders<User>.IndexKeys.Ascending(u => u.Email),
                new CreateIndexOptions { Unique = true }),
            new CreateIndexModel<User>(
                Builders<User>.IndexKeys.Ascending(u => u.Username),
                new CreateIndexOptions { Unique = true })
        ]);

        RefreshTokens.Indexes.CreateMany([
            new CreateIndexModel<RefreshToken>(
                Builders<RefreshToken>.IndexKeys.Ascending(rt => rt.Token),
                new CreateIndexOptions { Unique = true }),
            new CreateIndexModel<RefreshToken>(
                Builders<RefreshToken>.IndexKeys.Ascending(rt => rt.UserId))
        ]);

        ApiKeys.Indexes.CreateMany([
            new CreateIndexModel<ApiKey>(
                Builders<ApiKey>.IndexKeys.Ascending(ak => ak.Key),
                new CreateIndexOptions { Unique = true }),
            new CreateIndexModel<ApiKey>(
                Builders<ApiKey>.IndexKeys.Ascending(ak => ak.UserId))
        ]);
    }
}
