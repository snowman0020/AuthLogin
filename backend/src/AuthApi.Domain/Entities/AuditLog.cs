namespace AuthApi.Domain.Entities;

public class AuditLog
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>HTTP Method (GET, POST, DELETE …)</summary>
    public string Method { get; set; } = string.Empty;

    /// <summary>Request path e.g. /api/auth/login</summary>
    public string Path { get; set; } = string.Empty;

    /// <summary>HTTP response status code</summary>
    public int StatusCode { get; set; }

    /// <summary>Response time in milliseconds</summary>
    public long DurationMs { get; set; }

    /// <summary>Id of the authenticated user (if any)</summary>
    public string? UserId { get; set; }

    /// <summary>Email of the authenticated user (if any)</summary>
    public string? UserEmail { get; set; }

    /// <summary>Client IP address</summary>
    public string? IpAddress { get; set; }

    /// <summary>User-Agent header</summary>
    public string? UserAgent { get; set; }

    /// <summary>Request body (truncated, sensitive fields removed)</summary>
    public string? RequestBody { get; set; }

    /// <summary>Auth method used: Bearer | ApiKey | Anonymous</summary>
    public string AuthMethod { get; set; } = "Anonymous";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
