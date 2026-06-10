using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SyncroLife.AIRecommendation;
using SyncroLife.Interfaces.Repositories;

namespace SyncroLife.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class RecommendationTestController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly IScheduleRepository _scheduleRepository;
        private readonly IGoalRepository _goalRepository;
        private readonly IHabitRepository _habitRepository;
        private readonly IRecommendationEngine _recommendationEngine;

        public RecommendationTestController(
            IUserRepository userRepository,
            IScheduleRepository scheduleRepository,
            IGoalRepository goalRepository,
            IHabitRepository habitRepository,
            IRecommendationEngine recommendationEngine)
        {
            _userRepository = userRepository;
            _scheduleRepository = scheduleRepository;
            _goalRepository = goalRepository;
            _habitRepository = habitRepository;
            _recommendationEngine = recommendationEngine;
        }

        [HttpGet("test")]
        public async Task<IActionResult> GenerateAsync()
        {
            var userId =
                Guid.Parse(
                    User.FindFirst("sub")!.Value);

            var user =
                await _userRepository
                    .GetByIdAsync(userId);

            if (user == null)
            {
                return NotFound("User not found.");
            }

            var schedules =
                await _scheduleRepository
                    .GetByUserIdAsync(userId);

            var goals =
                await _goalRepository
                    .GetByUserIdAsync(userId);

            var habits =
                await _habitRepository
                    .GetByUserIdAsync(userId);

            var context = new RecommendationContext
            {
                User = user,
                Schedules = schedules,
                Goals = goals,
                Habits = habits,
                CurrentTime = DateTime.UtcNow
            };

            var results =
                await _recommendationEngine
                    .GenerateAsync(context);

            return Ok(results);
        }
    }
}