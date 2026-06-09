using SyncroLife.Models;

namespace SyncroLife.Interfaces.Repositories
{
    public interface IReminderRepository
    {
        Task<Reminder?> GetByIdAsync(Guid reminderId);

        Task<List<Reminder>> GetByUserIdAsync(Guid userId);

        Task AddAsync(Reminder reminder);

        Task UpdateAsync(Reminder reminder);

        Task DeleteAsync(Reminder reminder);

        Task<bool> ExistsAsync(Guid reminderId);
        Task<List<Reminder>> GetPendingRemindersAsync(DateTime utcNow);
    }
}