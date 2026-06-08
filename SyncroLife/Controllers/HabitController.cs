using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SyncroLife.DTOs.Habit;
using SyncroLife.Interfaces.Services;

namespace SyncroLife.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Customer")]
    public class HabitController : ControllerBase
    {
        private readonly IHabitService _habitService;

        public HabitController(IHabitService habitService)
        {
            _habitService = habitService;
        }

        [HttpPost("create-habits")]
        public async Task<IActionResult> CreateHabit(CreateHabitDTO request)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirst("userId")!.Value);

                var result = await _habitService.CreateHabitAsync(userId, request);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("my-habits")]
        public async Task<IActionResult> GetMyHabits()
        {
            try
            {
                var userId = Guid.Parse(User.FindFirst("sub")!.Value);

                var result = await _habitService.GetHabitsByUserAsync(userId);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("detail{habitId}")]
        public async Task<IActionResult> GetHabitById(Guid habitId)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirst("sub")!.Value);

                var result = await _habitService.GetHabitByIdAsync(userId, habitId);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
        [HttpPut("update-habit{habitId}")]
        public async Task<IActionResult> UpdateHabit(Guid habitId, UpdateHabitDTO request)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirst("sub")!.Value);

                var result = await _habitService.UpdateHabitAsync(userId, habitId, request);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpDelete("{habitId}")]
        public async Task<IActionResult> DeleteHabit(Guid habitId)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirst("sub")!.Value);

                var result = await _habitService.DeleteHabitAsync(userId, habitId);

                return Ok(new
                {
                    Success = result,
                    Message = "Habit deleted successfully"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}