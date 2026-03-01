using AuthApi.Domain.Entities;
using AuthApi.Domain.Interfaces.Repositories;
using AuthApi.Domain.Interfaces.Services;
using Microsoft.Extensions.Configuration;

namespace AuthApi.Application.Services;

public class AuthService(
    IUserRepository userRepo,
    IRefreshTokenRepository refreshTokenRepo,
    IJwtService jwtService,
    IConfiguration config
) : IAuthService
{
    private readonly int _refreshTokenDays = int.Parse(config["Jwt:RefreshTokenDays"] ?? "7");

    public async Task<(User user, string accessToken, string refreshToken, DateTime expiresAt)>
        RegisterAsync(string username, string email, string password)
    {
        if (await userRepo.ExistsByEmailAsync(email))
            throw new InvalidOperationException("Email already registered");

        if (await userRepo.ExistsByUsernameAsync(username))
            throw new InvalidOperationException("Username already taken");

        var user = new User
        {
            Username = username,
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password)
        };

        await userRepo.CreateAsync(user);
        return await IssueTokensAsync(user);
    }

    public async Task<(User user, string accessToken, string refreshToken, DateTime expiresAt)>
        LoginAsync(string email, string password)
    {
        var user = await userRepo.GetByEmailAsync(email)
            ?? throw new UnauthorizedAccessException("Invalid email or password");

        if (!user.IsActive)
            throw new UnauthorizedAccessException("Account is deactivated");

        if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid email or password");

        return await IssueTokensAsync(user);
    }

    public async Task<(User user, string accessToken, string refreshToken, DateTime expiresAt)>
        RefreshTokenAsync(string refreshToken)
    {
        var token = await refreshTokenRepo.GetByTokenAsync(refreshToken)
            ?? throw new UnauthorizedAccessException("Invalid refresh token");

        if (!token.IsValid)
            throw new UnauthorizedAccessException(
                token.IsRevoked ? "Refresh token has been revoked" : "Refresh token has expired");

        var user = await userRepo.GetByIdAsync(token.UserId)
            ?? throw new UnauthorizedAccessException("User not found");

        if (!user.IsActive)
            throw new UnauthorizedAccessException("Account is deactivated");

        // Rotate: revoke old token
        await refreshTokenRepo.RevokeAsync(token.Id, "Replaced by new token");

        return await IssueTokensAsync(user);
    }

    public async Task LogoutAsync(string refreshToken)
    {
        var token = await refreshTokenRepo.GetByTokenAsync(refreshToken);
        if (token is not null && token.IsValid)
            await refreshTokenRepo.RevokeAsync(token.Id, "User logout");
    }

    public async Task LogoutAllAsync(string userId) =>
        await refreshTokenRepo.RevokeAllByUserAsync(userId, "All sessions revoked");

    // ─── Private ───────────────────────────────────────────────────────────────

    private async Task<(User, string, string, DateTime)> IssueTokensAsync(User user)
    {
        var accessToken = jwtService.GenerateAccessToken(user);
        var refreshTokenStr = jwtService.GenerateRefreshToken();
        var expiresAt = DateTime.UtcNow.AddDays(_refreshTokenDays);

        await refreshTokenRepo.CreateAsync(new RefreshToken
        {
            Token = refreshTokenStr,
            UserId = user.Id,
            ExpiresAt = expiresAt
        });

        return (user, accessToken, refreshTokenStr, expiresAt);
    }
}
