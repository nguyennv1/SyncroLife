using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SyncroLife.Interfaces.Services;

namespace SyncroLife.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Customer,Admin")]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService;
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

        [HttpGet]
        public async Task<IActionResult> GetNotifications()
        {
            var result =
                await _notificationService
                    .GetNotificationsAsync(
                        GetUserId());

            return Ok(result);
        }

        [HttpGet("unread")]
        public async Task<IActionResult> GetUnreadNotifications()
        {
            var result =
                await _notificationService
                    .GetUnreadNotificationsAsync(
                        GetUserId());

            return Ok(result);
        }

        [HttpGet("unread-count")]
        public async Task<IActionResult> GetUnreadCount()
        {
            var result =
                await _notificationService
                    .GetUnreadCountAsync(
                        GetUserId());

            return Ok(result);
        }

        [HttpPut("{notificationId}/read")]
        public async Task<IActionResult> MarkAsRead(Guid notificationId)
        {
            await _notificationService
                .MarkAsReadAsync(
                    GetUserId(),
                    notificationId);

            return NoContent();
        }

        [HttpPut("read-all")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            await _notificationService
                .MarkAllAsReadAsync(
                    GetUserId());

            return NoContent();
        }
    }
}