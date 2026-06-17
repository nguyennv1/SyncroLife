using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using SyncroLife.DTOs.DeviceToken;
using SyncroLife.Interfaces.Services;

namespace SyncroLife.Controllers
{
    [Route("api/device-token")]
    [ApiController]
    [Authorize]
    public class DeviceTokenController : ControllerBase
    {
        private readonly IUserDeviceTokenService _service;

        public DeviceTokenController(
            IUserDeviceTokenService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<IActionResult> RegisterToken(
            RegisterDeviceTokenDTO dto)
        {
            var userIdClaim =
                User.FindFirst("sub")?.Value;

            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized();

            var userId = Guid.Parse(userIdClaim);

            await _service.RegisterTokenAsync(
                userId,
                dto);

            return Ok(new
            {
                Message = "Device token registered successfully"
            });
        }
    }
}