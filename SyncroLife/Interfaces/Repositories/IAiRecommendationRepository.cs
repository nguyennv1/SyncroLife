using SyncroLife.Models;

namespace SyncroLife.Interfaces.Repositories
{
    public interface IAiRecommendationRepository
    {
        Task AddAsync(AiRecommendation recommendation);

        Task<List<AiRecommendation>> GetByUserIdAsync(Guid userId);

        Task<AiRecommendation?> GetLatestAsync(Guid userId);

        Task<AiRecommendation?> GetByIdAsync(Guid recommendationId);

        Task UpdateAsync(AiRecommendation recommendation);
        Task<bool> ExistsRecentAsync(Guid userId, string recommendationType, Guid? scheduleId, Guid? goalId, Guid? habitId, Guid? mealId, Guid? preferenceId);
    }
}
