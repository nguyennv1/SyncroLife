using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SyncroLife.DTOs.Schedule;
using SyncroLife.Interfaces.Services;

namespace SyncroLife.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ScheduleController : ControllerBase
    {
        private readonly IScheduleService _scheduleService;

        public ScheduleController(IScheduleService scheduleService)
        {
            _scheduleService = scheduleService;
        }

        private Guid GetUserId()
        {
            var userIdClaim = User.FindFirst("sub");

            if (userIdClaim == null)
            {
                throw new Exception("UserId claim not found.");
            }

            return Guid.Parse(userIdClaim.Value);
        }

        [HttpPost]
        public async Task<IActionResult> CreateSchedule(CreateScheduleDTO request)
        {
            var result = await _scheduleService.CreateScheduleAsync(
                GetUserId(),
                request);

            return CreatedAtAction(
                nameof(GetScheduleById),
                new { scheduleId = result.ScheduleId },
                result);
        }

        [HttpGet("my-schedules")]
        public async Task<IActionResult> GetMySchedules()
        {
            var result = await _scheduleService.GetSchedulesByUserAsync(
                GetUserId());

            return Ok(result);
        }

        [HttpGet("{scheduleId}")]
        public async Task<IActionResult> GetScheduleById(Guid scheduleId)
        {
            var result = await _scheduleService.GetScheduleByIdAsync(
                GetUserId(),
                scheduleId);

            return Ok(result);
        }

        [HttpPut("{scheduleId}")]
        public async Task<IActionResult> UpdateSchedule(
            Guid scheduleId,
            UpdateScheduleDTO request)
        {
            var result = await _scheduleService.UpdateScheduleAsync(
                GetUserId(),
                scheduleId,
                request);

            return Ok(result);
        }

        [HttpDelete("{scheduleId}")]
        public async Task<IActionResult> DeleteSchedule(Guid scheduleId)
        {
            await _scheduleService.DeleteScheduleAsync(
                GetUserId(),
                scheduleId);

            return NoContent();
        }

        [HttpGet("today")]
        public async Task<IActionResult> GetTodaySchedules()
        {
            var result = await _scheduleService.GetTodaySchedulesAsync(
                GetUserId());

            return Ok(result);
        }

        [HttpGet("date/{date}")]
        public async Task<IActionResult> GetSchedulesByDate(DateTime date)
        {
            var result = await _scheduleService.GetSchedulesByDateAsync(
                GetUserId(),
                date);

            return Ok(result);
        }
    }
}