using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SyncroLife.DTOs.Recommendation;
using SyncroLife.Interfaces.Services;

namespace SyncroLife.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    [EnableRateLimiting("AiAnalysisPolicy")]
    public class RecommendationController : ControllerBase
    {
        private readonly IRecommendationService _recommendationService;

        public RecommendationController(IRecommendationService recommendationService)
        {
            _recommendationService = recommendationService;
        }

        [HttpPost("generate")]
        public async Task<IActionResult> Generate()
        {
            var userId = Guid.Parse(User.FindFirst("sub")!.Value);

            var recommendations =
                await _recommendationService
                    .GenerateForUserAsync(userId);

            return Ok(recommendations);
        }

        [HttpGet]
        public async Task<IActionResult> GetMyRecommendations()
        {
            var userId = Guid.Parse(User.FindFirst("sub")!.Value);

            var recommendations =
                await _recommendationService
                    .GetByUserAsync(userId);

            return Ok(recommendations);
        }

        [HttpGet("latest")]
        public async Task<IActionResult> GetLatest()
        {
            var userId = Guid.Parse(User.FindFirst("sub")!.Value);

            var recommendation =
                await _recommendationService
                    .GetLatestAsync(userId);

            if (recommendation == null)
            {
                return NotFound(
                    "No recommendation found.");
            }

            return Ok(recommendation);
        }

        [HttpPut("{recommendationId}/feedback")]
        public async Task<IActionResult> UpdateFeedback(
            Guid recommendationId,
            UpdateRecommendationFeedbackDTO dto)
        {
            var userId =Guid.Parse(User.FindFirst("sub")!.Value);

            await _recommendationService
                .UpdateFeedbackAsync(
                    userId,
                    recommendationId,
                    dto.Feedback);

            return Ok(new
            {
                message =
                    "Feedback updated successfully."
            });
        }
    }
}