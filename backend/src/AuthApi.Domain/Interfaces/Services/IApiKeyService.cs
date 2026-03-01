using AuthApi.Domain.Entities;

namespace AuthApi.Domain.Interfaces.Services;

public interface IApiKeyService
{
    Task<ApiKey> CreateAsync(string userId, string name, DateTime? expiresAt);
    Task<List<ApiKey>> GetAllAsync(string userId);
    Task<bool> RevokeAsync(string userId, string keyId);
    Task<User?> ValidateAsync(string key);
}
