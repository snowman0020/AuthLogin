using AuthApi.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using System.Security.Authentication;

namespace AuthApi.Infrastructure.Data;

public class MongoDbContext
{
    private readonly IMongoDatabase _db;
    private readonly ILogger<MongoDbContext>? _logger;

    public MongoDbContext(IConfiguration config, ILogger<MongoDbContext>? logger = null)
    {
        _logger = logger;

        var uri = config["MongoDB:Uri"]
            ?? throw new InvalidOperationException("MongoDB:Uri is not configured");
        var dbName = config["MongoDB:DatabaseName"] ?? "authdb";

        // Fix: Bypass Windows Schannel SSL/TLS auth failure (0x80090304 SEC_E_NO_AUTHENTICATING_AUTHORITY)
        var settings = MongoClientSettings.FromConnectionString(uri);
        settings.SslSettings = new SslSettings
        {
            EnabledSslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13,
            CheckCertificateRevocation = false
        };
        // AllowInsecureTls overrides RemoteCertificateValidationCallback at the managed .NET level,
        // bypassing Windows Schannel's SSPI auth which fails with 0x80090304 on some Windows configs.
        settings.AllowInsecureTls = true;
        settings.ServerSelectionTimeout = TimeSpan.FromSeconds(15);

        _db = new MongoClient(settings).GetDatabase(dbName);

        // Fix: Run index creation in background — never block the constructor
        _ = Task.Run(EnsureIndexesAsync);
    }

    public IMongoCollection<User> Users => _db.GetCollection<User>("users");
    public IMongoCollection<RefreshToken> RefreshTokens => _db.GetCollection<RefreshToken>("refreshTokens");
    public IMongoCollection<ApiKey> ApiKeys => _db.GetCollection<ApiKey>("apiKeys");
    public IMongoCollection<AuditLog> AuditLogs => _db.GetCollection<AuditLog>("auditLogs");

    // Called externally (e.g. IHostedService) to await completion if needed
    public async Task EnsureIndexesAsync()
    {
        try
        {
            await Users.Indexes.CreateManyAsync([
                new CreateIndexModel<User>(
                    Builders<User>.IndexKeys.Ascending(u => u.Email),
                    new CreateIndexOptions { Unique = true }),
                new CreateIndexModel<User>(
                    Builders<User>.IndexKeys.Ascending(u => u.Username),
                    new CreateIndexOptions { Unique = true })
            ]);

            await RefreshTokens.Indexes.CreateManyAsync([
                new CreateIndexModel<RefreshToken>(
                    Builders<RefreshToken>.IndexKeys.Ascending(rt => rt.Token),
                    new CreateIndexOptions { Unique = true }),
                new CreateIndexModel<RefreshToken>(
                    Builders<RefreshToken>.IndexKeys.Ascending(rt => rt.UserId))
            ]);

            await ApiKeys.Indexes.CreateManyAsync([
                new CreateIndexModel<ApiKey>(
                    Builders<ApiKey>.IndexKeys.Ascending(ak => ak.Key),
                    new CreateIndexOptions { Unique = true }),
                new CreateIndexModel<ApiKey>(
                    Builders<ApiKey>.IndexKeys.Ascending(ak => ak.UserId))
            ]);

            await AuditLogs.Indexes.CreateManyAsync([
                new CreateIndexModel<AuditLog>(
                    Builders<AuditLog>.IndexKeys.Descending(l => l.CreatedAt)),
                new CreateIndexModel<AuditLog>(
                    Builders<AuditLog>.IndexKeys.Ascending(l => l.UserId)),
                new CreateIndexModel<AuditLog>(
                    Builders<AuditLog>.IndexKeys.Ascending(l => l.Path))
            ]);

            _logger?.LogInformation("MongoDB indexes ensured successfully");
        }
        catch (Exception ex)
        {
            // Log but never crash the app — indexes can be created later
            _logger?.LogWarning(ex, "MongoDB index creation failed: {Message}", ex.Message);
        }
    }
}
