namespace AuthApi.Domain.Entities;

public class RefreshToken
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Token { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsRevoked { get; set; } = false;
    public string? RevokedReason { get; set; }

    public bool IsExpired => ExpiresAt < DateTime.UtcNow;
    public bool IsValid => !IsRevoked && !IsExpired;
}
