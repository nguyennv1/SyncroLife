using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SyncroLife.DTOs.Reminder;
using SyncroLife.Interfaces.Services;

namespace SyncroLife.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Customer")]
    public class ReminderController : ControllerBase
    {
        private readonly IReminderService _reminderService;

        public ReminderController( IReminderService reminderService)
        {
            _reminderService = reminderService;
        }

        private Guid GetUserId()
        {
            var userIdClaim = User.FindFirst("sub");

            if (userIdClaim == null)
            {
                throw new Exception(
                    "UserId claim not found.");
            }

            return Guid.Parse(userIdClaim.Value);
        }

        [HttpPost]
        public async Task<IActionResult> CreateReminder( CreateReminderDTO request)
        {
            var result =
                await _reminderService.CreateAsync(
                    GetUserId(),
                    request);

            return CreatedAtAction(
                nameof(GetReminderById),
                new { reminderId = result.ReminderId },
                result);
        }

        [HttpGet("my-reminders")]
        public async Task<IActionResult> GetMyReminders()
        {
            var result =
                await _reminderService.GetMyRemindersAsync(
                    GetUserId());

            return Ok(result);
        }

        [HttpGet("reminder-detail{reminderId}")]
        public async Task<IActionResult> GetReminderById(Guid reminderId)
        {
            var result =
                await _reminderService.GetByIdAsync(
                    GetUserId(),
                    reminderId);

            return Ok(result);
        }

        [HttpPut("update-reminder{reminderId}")]
        public async Task<IActionResult> UpdateReminder(Guid reminderId, UpdateReminderDTO request)
        {
            var result =
                await _reminderService.UpdateAsync(
                    GetUserId(),
                    reminderId,
                    request);

            return Ok(result);
        }

        [HttpDelete("delete-reminder{reminderId}")]
        public async Task<IActionResult> DeleteReminder( Guid reminderId)
        {
            await _reminderService.DeleteAsync(
                GetUserId(),
                reminderId);

            return NoContent();
        }
    }
}