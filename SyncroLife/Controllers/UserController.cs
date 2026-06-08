using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SyncroLife.DTOs.User;
using SyncroLife.Interfaces.Services;

namespace SyncroLife.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet("user-details{userId}")]
    public async Task<IActionResult> GetProfile(Guid userId)
    {
        var result =
            await _userService.GetProfileAsync(userId);

        return Ok(result);
    }

    [HttpPut("update-user{userId}")]
    public async Task<IActionResult> UpdateProfile(
        Guid userId,
        UpdateProfileDTO request)
    {
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