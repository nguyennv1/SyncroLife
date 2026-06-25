using System;
using System.Collections.Generic;

namespace SyncroLife.Models;

public partial class Schedule
{
    public Guid ScheduleId { get; set; }

    public Guid UserId { get; set; }

    public Guid TypeId { get; set; }

    public string Title { get; set; } = null!;

    public DateTime StartTime { get; set; }

    public DateTime EndTime { get; set; }

    public string? Description { get; set; }

    public bool? IsCompleted { get; set; }

    public bool? IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public string? GoogleEventId { get; set; }

    public virtual ICollection<AiRecommendation> AiRecommendations { get; set; } = new List<AiRecommendation>();

    public virtual ICollection<Reminder> Reminders { get; set; } = new List<Reminder>();

    public virtual ScheduleType Type { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
