using System;
using System.Collections.Generic;

namespace SyncroLife.Models;

public partial class UserDailySchedule
{
    public Guid? ScheduleId { get; set; }

    public Guid? UserId { get; set; }

    public string? Username { get; set; }

    public string? TypeName { get; set; }

    public string? EnergyLevel { get; set; }

    public string? Title { get; set; }

    public DateTime? StartTime { get; set; }

    public DateTime? EndTime { get; set; }

    public string? Description { get; set; }
}
