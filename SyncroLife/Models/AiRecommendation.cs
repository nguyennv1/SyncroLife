using System;
using System.Collections.Generic;

namespace SyncroLife.Models;

public partial class AiRecommendation
{
    public Guid RecommendationId { get; set; }

    public Guid UserId { get; set; }

    public Guid? ScheduleId { get; set; }

    public Guid? GoalId { get; set; }

    public Guid? HabitId { get; set; }

    public Guid? PreferenceId { get; set; }

    public Guid? MealId { get; set; }

    public string RecommendationType { get; set; } = null!;

    public string? Reasoning { get; set; }

    public string? ContextSnapshot { get; set; }

    public decimal? Score { get; set; }

    public string? UserFeedback { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Goal? Goal { get; set; }

    public virtual Habit? Habit { get; set; }

    public virtual Meal? Meal { get; set; }

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    public virtual DietaryPreference? Preference { get; set; }

    public virtual Schedule? Schedule { get; set; }

    public virtual User User { get; set; } = null!;
}
