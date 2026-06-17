using SyncroLife.DTOs.Recommendation;
using SyncroLife.Models;

namespace SyncroLife.Interfaces.Services
{
    public interface IRecommendationService
    {
        Task<List<RecommendationResponseDTO>> GenerateForUserAsync(Guid userId);

        Task<List<RecommendationResponseDTO>> GetByUserAsync(Guid userId);

        Task<RecommendationResponseDTO?> GetLatestAsync(Guid userId);

        Task UpdateFeedbackAsync(Guid userId, Guid recommendationId, string feedback);
    }
}