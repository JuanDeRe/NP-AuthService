using System.Security.Claims;
using AuthService.Models;
using AuthService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.Controllers;

[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public ActionResult<LoginResponse> Login([FromBody] LoginRequest request)
    {
        var response = _authService.Login(request);

        if (response is null)
        {
            return Unauthorized(new
            {
                message = "Invalid username or password"
            });
        }

        return Ok(response);
    }

    [Authorize]
    [HttpGet("me")]
    public IActionResult Me()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var username = User.FindFirst(ClaimTypes.Name)?.Value;
        var role = User.FindFirst(ClaimTypes.Role)?.Value;

        return Ok(new
        {
            userId,
            username,
            role
        });
    }

    [Authorize(Roles = "admin")]
    [HttpGet("admin-check")]
    public IActionResult AdminCheck()
    {
        return Ok(new
        {
            message = "Admin access granted"
        });
    }
}