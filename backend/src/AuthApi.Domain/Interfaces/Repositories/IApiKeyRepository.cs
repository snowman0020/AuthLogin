using AuthApi.Domain.Entities;

namespace AuthApi.Domain.Interfaces.Repositories;

public interface IApiKeyRepository
{
    Task<ApiKey?> GetByKeyAsync(string key);
    Task<ApiKey?> GetByIdAndUserAsync(string id, string userId);
    Task<List<ApiKey>> GetAllByUserAsync(string userId);
    Task CreateAsync(ApiKey apiKey);
    Task DeactivateAsync(string id);
    Task UpdateLastUsedAsync(string id);
}
