using System;
using System.Collections.Generic;

namespace SyncroLife.Models;

public partial class Reminder
{
    public Guid ReminderId { get; set; }

    public Guid ScheduleId { get; set; }

    public DateTime RemindAt { get; set; }

    public string? ReminderType { get; set; }

    public bool? IsSent { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    public virtual Schedule Schedule { get; set; } = null!;
}
