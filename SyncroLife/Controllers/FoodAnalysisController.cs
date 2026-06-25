using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SyncroLife.Interfaces.Services;
using System.Security.Claims;

namespace SyncroLife.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class FoodAnalysisController : ControllerBase
{
    private readonly IFoodAnalysisService _foodAnalysisService;

    public FoodAnalysisController(
        IFoodAnalysisService foodAnalysisService)
    {
        _foodAnalysisService = foodAnalysisService;
    }

    [HttpPost("analyze")]
    public async Task<IActionResult> AnalyzeFood(
        IFormFile image)
    {
        var userIdClaim =
            User.FindFirst("sub")?.Value;

        if (string.IsNullOrWhiteSpace(userIdClaim))
        {
            return Unauthorized("UserId not found.");
        }

        var userId = Guid.Parse(userIdClaim);

        var result =
            await _foodAnalysisService
                .AnalyzeFoodAsync(
                    userId,
                    image);

        return Ok(result);
    }
}