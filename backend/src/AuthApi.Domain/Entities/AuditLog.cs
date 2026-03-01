using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace AuthApi.Domain.Entities;

public class AuditLog
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    /// <summary>HTTP Method (GET, POST, DELETE …)</summary>
    [BsonElement("method")]
    public string Method { get; set; } = string.Empty;

    /// <summary>Request path e.g. /api/auth/login</summary>
    [BsonElement("path")]
    public string Path { get; set; } = string.Empty;

    /// <summary>HTTP response status code</summary>
    [BsonElement("statusCode")]
    public int StatusCode { get; set; }

    /// <summary>Response time in milliseconds</summary>
    [BsonElement("durationMs")]
    public long DurationMs { get; set; }

    /// <summary>MongoDB ObjectId of the authenticated user (if any)</summary>
    [BsonElement("userId")]
    public string? UserId { get; set; }

    /// <summary>Email of the authenticated user (if any)</summary>
    [BsonElement("userEmail")]
    public string? UserEmail { get; set; }

    /// <summary>Client IP address</summary>
    [BsonElement("ipAddress")]
    public string? IpAddress { get; set; }

    /// <summary>User-Agent header</summary>
    [BsonElement("userAgent")]
    public string? UserAgent { get; set; }

    /// <summary>Request body (truncated, sensitive fields removed)</summary>
    [BsonElement("requestBody")]
    public string? RequestBody { get; set; }

    /// <summary>Auth method used: Bearer | ApiKey | Anonymous</summary>
    [BsonElement("authMethod")]
    public string AuthMethod { get; set; } = "Anonymous";

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
