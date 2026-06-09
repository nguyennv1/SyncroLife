using SyncroLife.DTOs.Notification;
using SyncroLife.Interfaces.Repositories;
using SyncroLife.Interfaces.Services;
using SyncroLife.Models;

namespace SyncroLife.Services
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _notificationRepository;

        public NotificationService(
            INotificationRepository notificationRepository)
        {
            _notificationRepository = notificationRepository;
        }

        public async Task<List<NotificationResponseDTO>>GetNotificationsAsync(Guid userId)
        {
            var notifications =
                await _notificationRepository
                    .GetByUserIdAsync(userId);

            return notifications.Select(x =>
                new NotificationResponseDTO
                {
                    NotificationId = x.NotificationId,
                    Content = x.Content,
                    NotificationType = x.NotificationType ?? "",
                    Status = x.Status ?? "",
                    IsRead = x.IsRead ?? false,
                    SentAt = x.SentAt,
                    ReadAt = x.ReadAt,
                    CreatedAt = x.CreatedAt
                }).ToList();
        }

        public async Task<List<NotificationResponseDTO>>GetUnreadNotificationsAsync(Guid userId)
        {
            var notifications =
                await _notificationRepository
                    .GetUnreadByUserIdAsync(userId);

            return notifications.Select(x =>
                new NotificationResponseDTO
                {
                    NotificationId = x.NotificationId,
                    Content = x.Content,
                    NotificationType = x.NotificationType ?? "",
                    Status = x.Status ?? "",
                    IsRead = x.IsRead ?? false,
                    SentAt = x.SentAt,
                    ReadAt = x.ReadAt,
                    CreatedAt = x.CreatedAt
                }).ToList();
        }

        public async Task<NotificationCountDTO>GetUnreadCountAsync(Guid userId)
        {
            var count =
                await _notificationRepository
                    .GetUnreadCountAsync(userId);

            return new NotificationCountDTO
            {
                Count = count
            };
        }

        public async Task MarkAsReadAsync(Guid userId, Guid notificationId)
        {
            var notification =
                await _notificationRepository
                    .GetByIdAsync(notificationId);

            if (notification == null)
            {
                throw new Exception(
                    "Notification not found.");
            }

            if (notification.UserId != userId)
            {
                throw new Exception(
                    "You are not allowed to access this notification.");
            }

            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;

            await _notificationRepository
                .UpdateAsync(notification);
        }

        public async Task MarkAllAsReadAsync(Guid userId)
        {
            var notifications =
                await _notificationRepository
                    .GetUnreadByUserIdAsync(userId);

            foreach (var notification in notifications)
            {
                notification.IsRead = true;
                notification.ReadAt = DateTime.UtcNow;

                await _notificationRepository
                    .UpdateAsync(notification);
            }
        }

        public async Task CreateReminderNotificationAsync(Reminder reminder)
        {
            var notification = new Notification
            {
                NotificationId = Guid.NewGuid(),
                UserId = reminder.Schedule.UserId,
                ReminderId = reminder.ReminderId,
                Content = $"Đã đến giờ chuẩn bị cho {reminder.Schedule.Title}",
                NotificationType = "reminder",
                Channel = "in_app",
                Status = "sent",
                IsRead = false,
                SentAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };

            await _notificationRepository.AddAsync(notification);
        }
    }
}