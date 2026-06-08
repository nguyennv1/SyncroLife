using System;
using System.Collections.Generic;

namespace SyncroLife.Models;

public partial class MealPlanDetail
{
    public Guid DetailId { get; set; }

    public Guid MealPlanId { get; set; }

    public Guid MealId { get; set; }

    public string MealTime { get; set; } = null!;

    public int? Quantity { get; set; }

    public string? Notes { get; set; }

    public bool? IsCompleted { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Meal Meal { get; set; } = null!;

    public virtual MealPlan MealPlan { get; set; } = null!;
}
