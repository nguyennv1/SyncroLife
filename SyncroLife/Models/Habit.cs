using System;
using System.Collections.Generic;

namespace SyncroLife.Models;

public partial class Habit
{
    public Guid HabitId { get; set; }

    public Guid UserId { get; set; }

    public string HabitName { get; set; } = null!;

    public string? HabitType { get; set; }

    public bool? IsPositive { get; set; }

    public int? FrequencyCount { get; set; }

    public string? FrequencyUnit { get; set; }

    public DateOnly? StartDate { get; set; }

    public bool? IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<AiRecommendation> AiRecommendations { get; set; } = new List<AiRecommendation>();

    public virtual User User { get; set; } = null!;
}
