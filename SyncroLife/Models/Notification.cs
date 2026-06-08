using System;
using System.Collections.Generic;

namespace SyncroLife.Models;

public partial class Notification
{
    public Guid NotificationId { get; set; }

    public Guid UserId { get; set; }

    public Guid? ReminderId { get; set; }

    public Guid? RecommendationId { get; set; }

    public string Content { get; set; } = null!;

    public string? NotificationType { get; set; }

    public string? Channel { get; set; }

    public string? Status { get; set; }

    public bool? IsRead { get; set; }

    public DateTime? SentAt { get; set; }

    public DateTime? ReadAt { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual AiRecommendation? Recommendation { get; set; }

    public virtual Reminder? Reminder { get; set; }

    public virtual User User { get; set; } = null!;
}
