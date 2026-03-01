using AuthApi.Domain.Entities;

namespace AuthApi.Domain.Interfaces.Repositories;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> GetByTokenAsync(string token);
    Task CreateAsync(RefreshToken token);
    Task RevokeAsync(string tokenId, string reason);
    Task RevokeAllByUserAsync(string userId, string reason);
}
