using AuthApi.Domain.Entities;
using AuthApi.Domain.Interfaces.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AuthApi.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AuditLogController(IAuditLogRepository auditLogRepo) : ControllerBase
{
    /// <summary>
    /// [Admin] Get all audit logs (paginated)
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(AuditLogsResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int limit = 50,
        [FromQuery] int skip = 0)
    {
        limit = Math.Clamp(limit, 1, 200);
        var logs = await auditLogRepo.GetAllAsync(limit, skip);
        var total = await auditLogRepo.CountAsync();
        return Ok(new AuditLogsResponse(logs, total, skip, limit));
    }

    /// <summary>
    /// Get audit logs for the current authenticated user
    /// </summary>
    [HttpGet("my")]
    [Authorize]
    [ProducesResponseType(typeof(List<AuditLog>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMine([FromQuery] int limit = 50)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        limit = Math.Clamp(limit, 1, 200);
        var logs = await auditLogRepo.GetByUserAsync(userId, limit);
        return Ok(logs);
    }

    /// <summary>
    /// [Admin] Get audit logs filtered by path (e.g. /api/auth/login)
    /// </summary>
    [HttpGet("path")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(List<AuditLog>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByPath(
        [FromQuery] string path,
        [FromQuery] int limit = 50)
    {
        if (string.IsNullOrWhiteSpace(path))
            return BadRequest(new { message = "path query param is required" });

        limit = Math.Clamp(limit, 1, 200);
        var logs = await auditLogRepo.GetByPathAsync(path, limit);
        return Ok(logs);
    }
}

public record AuditLogsResponse(
    List<AuditLog> Data,
    long Total,
    int Skip,
    int Limit);
