using System;
using System.Collections.Generic;

namespace SyncroLife.Models;

public partial class FoodAnalysis
{
    public Guid AnalysisId { get; set; }

    public Guid UserId { get; set; }

    public Guid? MealId { get; set; }

    public string? ImageUrl { get; set; }

    public string? DetectedMealName { get; set; }

    public decimal? ConfidenceScore { get; set; }

    public int? Calories { get; set; }

    public decimal? Protein { get; set; }

    public decimal? Carbs { get; set; }

    public decimal? Fats { get; set; }

    public decimal? Fiber { get; set; }

    public decimal? Sodium { get; set; }

    public string? AnalysisResult { get; set; }

    public string? AiNotes { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Meal? Meal { get; set; }

    public virtual User User { get; set; } = null!;
}
