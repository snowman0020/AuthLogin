using AuthApi.Domain.Entities;
using AuthApi.Domain.Interfaces.Repositories;
using AuthApi.Domain.Interfaces.Services;
using System.Security.Cryptography;

namespace AuthApi.Application.Services;

public class ApiKeyService(
    IApiKeyRepository apiKeyRepo,
    IUserRepository userRepo
) : IApiKeyService
{
    public async Task<ApiKey> CreateAsync(string userId, string name, DateTime? expiresAt)
    {
        var apiKey = new ApiKey
        {
            Key = GenerateKey(),
            Name = name,
            UserId = userId,
            ExpiresAt = expiresAt
        };

        await apiKeyRepo.CreateAsync(apiKey);
        return apiKey;
    }

    public async Task<List<ApiKey>> GetAllAsync(string userId) =>
        await apiKeyRepo.GetAllByUserAsync(userId);

    public async Task<bool> RevokeAsync(string userId, string keyId)
    {
        var key = await apiKeyRepo.GetByIdAndUserAsync(keyId, userId);
        if (key is null) return false;

        await apiKeyRepo.DeactivateAsync(keyId);
        return true;
    }

    public async Task<User?> ValidateAsync(string key)
    {
        var apiKey = await apiKeyRepo.GetByKeyAsync(key);
        if (apiKey is null || !apiKey.IsValid) return null;

        var user = await userRepo.GetByIdAsync(apiKey.UserId);
        if (user is null || !user.IsActive) return null;

        await apiKeyRepo.UpdateLastUsedAsync(apiKey.Id);
        return user;
    }

    private static string GenerateKey()
    {
        var bytes = new byte[32];
        RandomNumberGenerator.Fill(bytes);
        return $"ak_{Convert.ToBase64String(bytes).Replace("+", "").Replace("/", "").Replace("=", "")}";
    }
}
