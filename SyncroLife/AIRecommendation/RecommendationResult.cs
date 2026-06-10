namespace SyncroLife.AIRecommendation
{
    public class RecommendationResult
    {
        public string RecommendationType { get; set; } = null!;

        public string Reasoning { get; set; } = null!;

        public decimal Score { get; set; }

        public Guid? ScheduleId { get; set; }

        public Guid? HabitId { get; set; }

        public Guid? GoalId { get; set; }

        public string? ContextSnapshot { get; set; }
    }
}
