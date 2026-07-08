using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SyncroLife.DTOs.User;
using SyncroLife.Interfaces.Services;

namespace SyncroLife.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet("user-details/{userId}")]
    public async Task<IActionResult> GetProfile(Guid userId)
    {
        var userIdClaim = User.FindFirst("sub")?.Value;
        if (string.IsNullOrWhiteSpace(userIdClaim))
        {
            return Unauthorized("Unauthorized access.");
        }
        var loggedInUserId = Guid.Parse(userIdClaim);
        var userRoleClaim = User.FindFirst(ClaimTypes.Role)?.Value ?? User.FindFirst("role")?.Value;
        bool isAdmin = userRoleClaim == "Admin";
        
        if (loggedInUserId != userId && !isAdmin)
        {
            return StatusCode(403, "You do not have permission to access this profile.");
        }

        var result =
            await _userService.GetProfileAsync(userId);

        return Ok(result);
    }

    [HttpPut("update-user/{userId}")]
    public async Task<IActionResult> UpdateProfile(
        Guid userId,
        UpdateProfileDTO request)
    {
        var userIdClaim = User.FindFirst("sub")?.Value;
        if (string.IsNullOrWhiteSpace(userIdClaim))
        {
            return Unauthorized("Unauthorized access.");
        }
        var loggedInUserId = Guid.Parse(userIdClaim);
        var userRoleClaim = User.FindFirst(ClaimTypes.Role)?.Value ?? User.FindFirst("role")?.Value;
        bool isAdmin = userRoleClaim == "Admin";
        
        if (loggedInUserId != userId && !isAdmin)
        {
            return StatusCode(403, "You do not have permission to update this profile.");
        }

        var result =
            await _userService.UpdateProfileAsync(
                userId,
                request);

        return Ok(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("all-user")]
    public async Task<IActionResult> GetAllUsers()
    {
        var users =
            await _userService.GetAllUsersAsync();

        return Ok(users);
    }
}