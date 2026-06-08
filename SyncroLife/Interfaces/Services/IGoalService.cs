using SyncroLife.DTOs.Goal;

namespace SyncroLife.Interfaces.Services
{
    public interface IGoalService
    {
        Task<GoalResponseDTO> CreateGoalAsync(Guid userId, CreateGoalDTO request);

        Task<List<GoalResponseDTO>> GetGoalsByUserAsync(Guid userId);

        Task<GoalResponseDTO?> GetGoalByIdAsync(Guid userId, Guid goalId);

        Task<bool> UpdateGoalAsync(Guid userId, Guid goalId, UpdateGoalDTO request);

        Task<bool> DeleteGoalAsync(Guid userId, Guid goalId);
    }
}
