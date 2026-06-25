using SyncroLife.Models;

namespace SyncroLife.Interfaces.Repositories
{
    public interface IFoodAnalysisRepository
    {
        Task AddAsync(FoodAnalysis analysis);

        Task<FoodAnalysis?> GetByIdAsync(Guid analysisId);

        Task<List<FoodAnalysis>> GetByUserIdAsync(Guid userId);
    }
}
