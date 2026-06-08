using SyncroLife.Models;

namespace SyncroLife.Interfaces.Repositories
{
    public interface IGoalRepository
    {
        Task AddAsync(Goal goal);

        Task<Goal?> GetByIdAsync(Guid goalId);

        Task<List<Goal>> GetByUserIdAsync(Guid userId);

        Task UpdateAsync(Goal goal);

        //Task DeleteAsync(Goal goal);    
    }
}
