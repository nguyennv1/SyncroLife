using System;
using System.Collections.Generic;

namespace SyncroLife.Models;

public partial class Goal
{
    public Guid GoalId { get; set; }

    public Guid UserId { get; set; }

    public string GoalType { get; set; } = null!;

    public string GoalName { get; set; } = null!;

    public decimal? TargetValue { get; set; }

    public decimal? CurrentValue { get; set; }

    public string? Unit { get; set; }

    public DateOnly StartDate { get; set; }

    public DateOnly? Deadline { get; set; }

    public bool? IsActive { get; set; }

    public bool? IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    public int? ProgressPercentage { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<AiRecommendation> AiRecommendations { get; set; } = new List<AiRecommendation>();

    public virtual User User { get; set; } = null!;
}
