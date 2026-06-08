using SyncroLife.Models;

namespace SyncroLife.Interfaces.Repositories
{
    public interface IHabitRepository
    {
        Task AddAsync(Habit habit);

        Task<Habit?> GetByIdAsync(Guid habitId);

        Task<List<Habit>> GetByUserIdAsync(Guid userId);

        Task UpdateAsync(Habit habit);
    }
}