using System;
using System.Collections.Generic;

namespace SyncroLife.Models;

public partial class MealPlan
{
    public Guid MealPlanId { get; set; }

    public Guid UserId { get; set; }

    public DateOnly PlanDate { get; set; }

    public string? PlanType { get; set; }

    public int? TotalCalories { get; set; }

    public decimal? TotalProtein { get; set; }

    public decimal? TotalCarbs { get; set; }

    public decimal? TotalFats { get; set; }

    public string? Notes { get; set; }

    public bool? IsCompleted { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<MealPlanDetail> MealPlanDetails { get; set; } = new List<MealPlanDetail>();

    public virtual User User { get; set; } = null!;
}
