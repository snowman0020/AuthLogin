using AuthApi.Application.DTOs;
using AuthApi.Domain.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AuthApi.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AuthController(IAuthService authService) : ControllerBase
{
    /// <summary>Register a new user account</summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest req)
    {
        try
        {
            var (user, access, refresh, exp) = await authService.RegisterAsync(
                req.Username, req.Email, req.Password);

            return CreatedAtAction(nameof(Register),
                new AuthResponse(access, refresh, exp,
                    new UserResponse(user.Id, user.Username, user.Email, user.Role)));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new MessageResponse(ex.Message));
        }
    }

    /// <summary>Login with email and password, returns JWT + Refresh Token</summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest req)
    {
        try
        {
            var (user, access, refresh, exp) = await authService.LoginAsync(req.Email, req.Password);

            return Ok(new AuthResponse(access, refresh, exp,
                new UserResponse(user.Id, user.Username, user.Email, user.Role)));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new MessageResponse(ex.Message));
        }
    }

    /// <summary>Refresh access token using refresh token (token rotation)</summary>
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest req)
    {
        try
        {
            var (user, access, refresh, exp) = await authService.RefreshTokenAsync(req.RefreshToken);

            return Ok(new AuthResponse(access, refresh, exp,
                new UserResponse(user.Id, user.Username, user.Email, user.Role)));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new MessageResponse(ex.Message));
        }
    }

    /// <summary>Logout from current session</summary>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Logout([FromBody] RefreshTokenRequest req)
    {
        await authService.LogoutAsync(req.RefreshToken);
        return Ok(new MessageResponse("Logged out successfully"));
    }

    /// <summary>Logout from all devices</summary>
    [HttpPost("logout-all")]
    [Authorize]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> LogoutAll()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        await authService.LogoutAllAsync(userId);
        return Ok(new MessageResponse("All sessions revoked"));
    }
}
