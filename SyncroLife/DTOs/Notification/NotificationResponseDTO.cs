namespace SyncroLife.DTOs.Notification
{
    public class NotificationResponseDTO
    {
        public Guid NotificationId { get; set; }

        public string Content { get; set; } = string.Empty;

        public string NotificationType { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public bool IsRead { get; set; }

        public DateTime? SentAt { get; set; }

        public DateTime? ReadAt { get; set; }

        public DateTime? CreatedAt { get; set; }
    }
}