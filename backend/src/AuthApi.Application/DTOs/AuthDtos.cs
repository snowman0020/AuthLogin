using System.ComponentModel.DataAnnotations;

namespace AuthApi.Application.DTOs;

// ─── Requests ─────────────────────────────────────────────────────────────────

public record RegisterRequest(
    [Required, MinLength(3), MaxLength(50)] string Username,
    [Required, EmailAddress] string Email,
    [Required, MinLength(6)] string Password
);

public record LoginRequest(
    [Required, EmailAddress] string Email,
    [Required] string Password
);

public record RefreshTokenRequest(
    [Required] string RefreshToken
);

public record CreateApiKeyRequest(
    [Required, MinLength(1), MaxLength(100)] string Name,
    DateTime? ExpiresAt
);

// ─── Responses ────────────────────────────────────────────────────────────────

public record AuthResponse(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt,
    UserResponse User
);

public record UserResponse(
    string Id,
    string Username,
    string Email,
    string Role
);

public record ApiKeyResponse(
    string Id,
    string Key,
    string Name,
    DateTime CreatedAt,
    DateTime? ExpiresAt,
    bool IsActive,
    DateTime? LastUsedAt
);

public record MessageResponse(string Message);
