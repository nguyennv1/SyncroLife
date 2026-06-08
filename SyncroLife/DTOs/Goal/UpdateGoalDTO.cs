namespace SyncroLife.DTOs.Goal
{
    public class UpdateGoalDTO
    {
        public string GoalType { get; set; } = null!;

        public string GoalName { get; set; } = null!;

        public decimal? TargetValue { get; set; }

        public string? Unit { get; set; }

        public DateOnly StartDate { get; set; }

        public DateOnly? Deadline { get; set; }

        public bool? IsActive { get; set; }
    }
}
