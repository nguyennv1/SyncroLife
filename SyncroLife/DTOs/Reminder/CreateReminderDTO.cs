using System.ComponentModel.DataAnnotations;

namespace SyncroLife.DTOs.Reminder
{
    public class CreateReminderDTO
    {
        [Required]
        public Guid ScheduleId { get; set; }

        [Required]
        public DateTime RemindAt { get; set; }

        [Required]
        public string ReminderType { get; set; } = null!;
    }
}