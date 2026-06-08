using System;
using System.Collections.Generic;

namespace SyncroLife.Models;

public partial class ScheduleType
{
    public Guid TypeId { get; set; }

    public string TypeName { get; set; } = null!;

    public string? Description { get; set; }

    public string? EnergyLevel { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();
}
