using AuthApi.API.Authentication;
using AuthApi.Application.DTOs;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AuthApi.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class UserController : ControllerBase
{
    /// <summary>Get current user profile — supports both JWT and API Key auth</summary>
    [HttpGet("me")]
    [Authorize(AuthenticationSchemes =
        $"{JwtBearerDefaults.AuthenticationScheme},{ApiKeyAuthHandler.SchemeName}")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult GetMe() => Ok(new UserResponse(
        User.FindFirstValue(ClaimTypes.NameIdentifier)!,
        User.FindFirstValue(ClaimTypes.Name)!,
        User.FindFirstValue(ClaimTypes.Email)!,
        User.FindFirstValue(ClaimTypes.Role)!
    ));

    /// <summary>Admin only endpoint</summary>
    [HttpGet("admin")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public IActionResult AdminOnly() =>
        Ok(new MessageResponse($"Hello Admin {User.FindFirstValue(ClaimTypes.Name)}!"));
}
