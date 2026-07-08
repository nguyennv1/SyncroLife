using SyncroLife.DTOs.FoodAnalysis;

namespace SyncroLife.Interfaces.Services
{
    public interface IFoodAnalysisService
    {
        Task<AnalyzeFoodResponse> AnalyzeFoodAsync(Guid userId, IFormFile image);
        Task<List<AnalyzeFoodResponse>> GetHistoryAsync(Guid userId);
    }
}
