using Microsoft.AspNetCore.Mvc;
using SyncroLife.DTOs.Auth;
using SyncroLife.Interfaces.Services;

namespace SyncroLife.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequestDTO request)
    {
        try {
            var result =
            await _authService.RegisterAsync(request);
            return Ok(result);
        }
        catch ( Exception e)
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