using AuthApi.Domain.Entities;
using System.Security.Claims;

namespace AuthApi.Domain.Interfaces.Services;

public interface IJwtService
{
    string GenerateAccessToken(User user);
    string GenerateRefreshToken();
    ClaimsPrincipal? ValidateToken(string token);
}
