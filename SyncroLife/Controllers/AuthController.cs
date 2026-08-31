using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SyncroLife.DTOs.Auth;
using SyncroLife.Interfaces.Services;

namespace SyncroLife.Controllers;

[ApiController]
[Route("api/[controller]")]
[EnableRateLimiting("AuthPolicy")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequestDTO request)
    {
        if (request == null)
        {
            return BadRequest("Request payload cannot be null.");
        }
        if (string.IsNullOrWhiteSpace(request.Username))
        {
            return BadRequest("Username cannot be empty or contain only whitespace.");
        }
        if (request.Username.Contains(" "))
        {
            return BadRequest("Username cannot contain spaces.");
        }
        if (string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest("Password cannot be empty or contain only whitespace.");
        }
        if (request.Password.Contains(" "))
        {
            return BadRequest("Password cannot contain spaces.");
        }
        if (request.Password.Length < 8)
        {
            return BadRequest("Password must be at least 8 characters long.");
        }

        try {
            var result = await _authService.RegisterAsync(request);
            return Ok(result);
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequestDTO request)
    {
        try
        {
            var result = await _authService.LoginAsync(request);

            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("google-login")]
    public async Task<IActionResult> GoogleLogin(GoogleLoginRequestDTO request)
    {
        try
        {
            var result = await _authService.GoogleLoginAsync(request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}