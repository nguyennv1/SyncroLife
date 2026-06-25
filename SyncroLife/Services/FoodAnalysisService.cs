using SyncroLife.DTOs.FoodAnalysis;
using SyncroLife.Interfaces.Repositories;
using SyncroLife.Interfaces.Services;
using SyncroLife.Models;

namespace SyncroLife.Services
{
    public class FoodAnalysisService : IFoodAnalysisService
    {
        private readonly IFoodAnalysisRepository _foodAnalysisRepository;
        private readonly IGeminiService _geminiService;

        public FoodAnalysisService(
            IFoodAnalysisRepository foodAnalysisRepository,
            IGeminiService geminiService)
        {
            _foodAnalysisRepository = foodAnalysisRepository;
            _geminiService = geminiService;
        }

        public async Task<AnalyzeFoodResponse> AnalyzeFoodAsync(Guid userId, IFormFile image)
        {
            if (image == null || image.Length == 0)
            {
                throw new Exception("Image is required.");
            }

            using var memoryStream = new MemoryStream();

            await image.CopyToAsync(memoryStream);

            var imageBytes = memoryStream.ToArray();

            var geminiResult =
                await _geminiService.AnalyzeFoodAsync(imageBytes);

            var analysis = new FoodAnalysis
            {
                AnalysisId = Guid.NewGuid(),

                UserId = userId,

                ImageUrl = null,

                DetectedMealName = geminiResult.FoodName,

                ConfidenceScore = geminiResult.Confidence,

                Calories = geminiResult.Calories,

                Protein = geminiResult.Protein,

                Carbs = geminiResult.Carbs,

                Fats = geminiResult.Fats,

                Fiber = geminiResult.Fiber,

                Sodium = geminiResult.Sodium,

                AnalysisResult = geminiResult.Description,

                AiNotes = geminiResult.AiNotes,

                CreatedAt = DateTime.UtcNow,

                UpdatedAt = DateTime.UtcNow
            };

            await _foodAnalysisRepository.AddAsync(analysis);

            return new AnalyzeFoodResponse
            {
                AnalysisId = analysis.AnalysisId,

                FoodName = geminiResult.FoodName,

                Confidence = geminiResult.Confidence,

                Calories = geminiResult.Calories,

                Protein = geminiResult.Protein,

                Carbs = geminiResult.Carbs,

                Fats = geminiResult.Fats,

                Fiber = geminiResult.Fiber,

                Sodium = geminiResult.Sodium,

                Description = geminiResult.Description,

                AiNotes = geminiResult.AiNotes,

                ImageUrl = analysis.ImageUrl,

                CreatedAt = analysis.CreatedAt ?? DateTime.UtcNow
            };
        }
    }
}
