namespace SyncroLife.DTOs.Recommendation
{
    public class RecommendationResponseDTO
    {
        public Guid RecommendationId { get; set; }

        public string RecommendationType { get; set; } = null!;

        public string? Reasoning { get; set; }

        public decimal? Score { get; set; }

        public Guid? ScheduleId { get; set; }

        public Guid? HabitId { get; set; }

        public Guid? GoalId { get; set; }
        public Guid? MealId { get; set; }

        public Guid? PreferenceId { get; set; }

        public string? UserFeedback { get; set; }

        public DateTime? CreatedAt { get; set; }
    }
}