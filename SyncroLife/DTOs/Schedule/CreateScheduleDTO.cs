namespace SyncroLife.DTOs.Schedule
{
    public class CreateScheduleDTO
    {
        public Guid TypeId { get; set; }

        public string Title { get; set; } = null!;

        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }

        public string? Description { get; set; }
    }
}
