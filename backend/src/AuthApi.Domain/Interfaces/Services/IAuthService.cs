using AuthApi.Domain.Entities;

namespace AuthApi.Domain.Interfaces.Services;

public interface IAuthService
{
    Task<(User user, string accessToken, string refreshToken, DateTime expiresAt)> RegisterAsync(
        string username, string email, string password);

    Task<(User user, string accessToken, string refreshToken, DateTime expiresAt)> LoginAsync(
        string email, string password);

    Task<(User user, string accessToken, string refreshToken, DateTime expiresAt)> RefreshTokenAsync(
        string refreshToken);

    Task LogoutAsync(string refreshToken);
    Task LogoutAllAsync(string userId);
}
