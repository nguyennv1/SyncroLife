namespace SyncroLife.DTOs.Reminder
{
    public class ReminderResponseDTO
    {
        public Guid ReminderId { get; set; }

        public Guid ScheduleId { get; set; }

        public string ScheduleTitle { get; set; } = null!;

        public DateTime RemindAt { get; set; }

        public string ReminderType { get; set; } = null!;

        public bool IsSent { get; set; }
    }
}