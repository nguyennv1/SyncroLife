using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SyncroLife.DTOs.Goal;
using SyncroLife.Interfaces.Services;
using System.Security.Claims;

namespace SyncroLife.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class GoalController : ControllerBase
    {
        private readonly IGoalService _goalService;

        public GoalController(IGoalService goalService)
        {
            _goalService = goalService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateGoal(CreateGoalDTO request)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

                var result = await _goalService.CreateGoalAsync(userId, request);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Message = ex.Message
                });
            }
        }

        [HttpGet("my-goals")]
        public async Task<IActionResult> GetMyGoals()
        {
            try
            {
                var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

                var result = await _goalService.GetGoalsByUserAsync(userId);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Message = ex.Message
                });
            }
        }

        [HttpGet("{goalId}")]
        public async Task<IActionResult> GetGoalById(Guid goalId)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

                var result = await _goalService.GetGoalByIdAsync(userId, goalId);

                if (result == null)
                {
                    return NotFound(new
                    {
                        Message = "Goal not found"
                    });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Message = ex.Message
                });
            }
        }

        [HttpPut("{goalId}")]
        public async Task<IActionResult> UpdateGoal(Guid goalId, UpdateGoalDTO request)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

                var success = await _goalService.UpdateGoalAsync(userId, goalId, request);

                if (!success)
                {
                    return NotFound(new
                    {
                        Message = "Goal not found"
                    });
                }

                return Ok(new
                {
                    Message = "Goal updated successfully"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Message = ex.Message
                });
            }
        }

        [HttpDelete("{goalId}")]
        public async Task<IActionResult> DeleteGoal(Guid goalId)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

                var success = await _goalService.DeleteGoalAsync(userId, goalId);

                if (!success)
                {
                    return NotFound(new
                    {
                        Message = "Goal not found"
                    });
                }

                return Ok(new
                {
                    Message = "Goal deleted successfully"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Message = ex.Message
                });
            }
        }
    }
}