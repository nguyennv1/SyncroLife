using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SyncroLife.Interfaces.Services;

namespace SyncroLife.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Customer,Admin,Manager")]
    public class ScheduleTypeController : ControllerBase
    {
        private readonly IScheduleTypeService _service;

        public ScheduleTypeController(
            IScheduleTypeService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAllAsync();

            return Ok(result);
        }
    }
}
