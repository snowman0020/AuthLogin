using AuthApi.Application.DTOs;
using AuthApi.Domain.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AuthApi.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class ApiKeyController(IApiKeyService apiKeyService) : ControllerBase
{
    private string UserId =>
        User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException();

    /// <summary>Create a new API key for the current user</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiKeyResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateApiKeyRequest req)
    {
        var key = await apiKeyService.CreateAsync(UserId, req.Name, req.ExpiresAt);
        var response = new ApiKeyResponse(
            key.Id, key.Key, key.Name, key.CreatedAt, key.ExpiresAt, key.IsActive, key.LastUsedAt);
        return Created($"/api/apikey/{key.Id}", response);
    }

    /// <summary>Get all API keys for the current user</summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<ApiKeyResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var keys = await apiKeyService.GetAllAsync(UserId);
        var response = keys.Select(k =>
            new ApiKeyResponse(k.Id, k.Key, k.Name, k.CreatedAt, k.ExpiresAt, k.IsActive, k.LastUsedAt));
        return Ok(response);
    }

    /// <summary>Revoke (deactivate) an API key</summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Revoke(string id)
    {
        var success = await apiKeyService.RevokeAsync(UserId, id);
        return success
            ? Ok(new MessageResponse("API key revoked"))
            : NotFound(new MessageResponse("API key not found"));
    }
}
