using System.ComponentModel.DataAnnotations;

namespace SyncroLife.DTOs.Reminder
{
    public class UpdateReminderDTO
    {
        [Required]
        public DateTime RemindAt { get; set; }

        [Required]
        public string ReminderType { get; set; } = null!;
    }
}