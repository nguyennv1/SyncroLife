using Microsoft.AspNetCore.Http;

namespace SyncroLife.DTOs.FoodAnalysis
{
    public class AnalyzeFoodRequest
    {
        public IFormFile Image { get; set; } = null!;
    }
}