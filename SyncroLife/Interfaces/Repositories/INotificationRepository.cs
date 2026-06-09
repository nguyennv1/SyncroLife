using SyncroLife.Models;

namespace SyncroLife.Interfaces.Repositories
{
    public interface INotificationRepository
    {
        Task<List<Notification>> GetByUserIdAsync(Guid userId);

        Task<List<Notification>> GetUnreadByUserIdAsync(Guid userId);

        Task<Notification?> GetByIdAsync(Guid notificationId);

        Task<int> GetUnreadCountAsync(Guid userId);
        Task AddAsync(Notification notification);

        Task UpdateAsync(Notification notification);
    }
}