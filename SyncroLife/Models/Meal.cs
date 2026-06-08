using System;
using System.Collections.Generic;

namespace SyncroLife.Models;

public partial class Meal
{
    public Guid MealId { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public string? Category { get; set; }

    public string? PortionSize { get; set; }

    public int? Calories { get; set; }

    public decimal? Protein { get; set; }

    public decimal? Carbs { get; set; }

    public decimal? Fats { get; set; }

    public decimal? Fiber { get; set; }

    public decimal? Sodium { get; set; }

    public bool? IsVegetarian { get; set; }

    public bool? IsVegan { get; set; }

    public string? Tags { get; set; }

    public string? ImageUrl { get; set; }

    public bool? IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<AiRecommendation> AiRecommendations { get; set; } = new List<AiRecommendation>();

    public virtual ICollection<FoodAnalysis> FoodAnalyses { get; set; } = new List<FoodAnalysis>();

    public virtual ICollection<MealPlanDetail> MealPlanDetails { get; set; } = new List<MealPlanDetail>();
}
