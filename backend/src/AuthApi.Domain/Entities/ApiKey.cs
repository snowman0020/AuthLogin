namespace AuthApi.Domain.Entities;

public class ApiKey
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Key { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ExpiresAt { get; set; }
    public DateTime? LastUsedAt { get; set; }

    public bool IsExpired => ExpiresAt.HasValue && ExpiresAt.Value < DateTime.UtcNow;
    public bool IsValid => IsActive && !IsExpired;
}
