using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SyncroLife.Interfaces.Services;

namespace SyncroLife.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GeminiTestController : ControllerBase
    {
        private readonly IGeminiService _geminiService;

        public GeminiTestController(
            IGeminiService geminiService)
        {
            _geminiService = geminiService;
        }

        [HttpPost]
        public async Task<IActionResult> Test(
            IFormFile image)
        {
            using var stream = new MemoryStream();

            await image.CopyToAsync(stream);

            var bytes = stream.ToArray();

            var result =
                await _geminiService.AnalyzeFoodAsync(bytes);

            return Ok(result);
        }
    }
}
