using System;
using System.Collections.Generic;

namespace SyncroLife.Models;

public partial class User
{
    public Guid UserId { get; set; }

    public Guid RoleId { get; set; }

    public string Username { get; set; } = null!;

    public string? Email { get; set; }

    public string PasswordHash { get; set; } = null!;

    public string? FullName { get; set; }

    public DateOnly? DateOfBirth { get; set; }

    public string? Gender { get; set; }

    public bool? IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<AiRecommendation> AiRecommendations { get; set; } = new List<AiRecommendation>();

    public virtual ICollection<FoodAnalysis> FoodAnalyses { get; set; } = new List<FoodAnalysis>();

    public virtual ICollection<Goal> Goals { get; set; } = new List<Goal>();

    public virtual ICollection<Habit> Habits { get; set; } = new List<Habit>();

    public virtual ICollection<MealPlan> MealPlans { get; set; } = new List<MealPlan>();

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    public virtual Role Role { get; set; } = null!;

    public virtual ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();

    public virtual ICollection<UserDietaryPreference> UserDietaryPreferences { get; set; } = new List<UserDietaryPreference>();

    public virtual UserSubscription? UserSubscription { get; set; }
}
