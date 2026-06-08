namespace SyncroLife.DTOs.Goal
{
    public class GoalResponseDTO
    {
        public Guid GoalId { get; set; }

        public string GoalType { get; set; } = null!;

        public string GoalName { get; set; } = null!;

        public decimal? TargetValue { get; set; }

        public decimal? CurrentValue { get; set; }

        public string? Unit { get; set; }

        public DateOnly StartDate { get; set; }

        public DateOnly? Deadline { get; set; }

        public int? ProgressPercentage { get; set; }

        public bool? IsActive { get; set; }

        public DateTime? CreatedAt { get; set; }
    }
}
