using SyncroLife.DTOs.Notification;
using SyncroLife.Models;

namespace SyncroLife.Interfaces.Services
{
    public interface INotificationService
    {
        Task<List<NotificationResponseDTO>> GetNotificationsAsync(Guid userId);

        Task<List<NotificationResponseDTO>> GetUnreadNotificationsAsync(Guid userId);

        Task<NotificationCountDTO> GetUnreadCountAsync(Guid userId);

        Task MarkAsReadAsync(Guid userId, Guid notificationId);

        Task MarkAllAsReadAsync(Guid userId);
        Task CreateReminderNotificationAsync(Reminder reminder);
        Task CreateRecommendationNotificationAsync(AiRecommendation recommendation);
    }
}