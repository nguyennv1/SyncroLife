namespace SyncroLife.DTOs.Schedule
{
    public class ScheduleResponseDTO
    {
        public Guid ScheduleId { get; set; }

        public Guid TypeId { get; set; }

        public string TypeName { get; set; } = null!;

        public string? EnergyLevel { get; set; }

        public string Title { get; set; } = null!;

        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }

        public string? Description { get; set; }

        public bool IsCompleted { get; set; }

        public DateTime? CreatedAt { get; set; }
    }
}
