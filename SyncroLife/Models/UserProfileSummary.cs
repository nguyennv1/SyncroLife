using System;
using System.Collections.Generic;

namespace SyncroLife.Models;

public partial class UserProfileSummary
{
    public Guid? UserId { get; set; }

    public string? Username { get; set; }

    public string? Email { get; set; }

    public int? Age { get; set; }

    public string? Gender { get; set; }

    public long? ActiveGoalsCount { get; set; }

    public long? HabitsCount { get; set; }

    public long? DietaryPreferencesCount { get; set; }

    public string? PlanName { get; set; }

    public DateOnly? SubscriptionEndDate { get; set; }
}
