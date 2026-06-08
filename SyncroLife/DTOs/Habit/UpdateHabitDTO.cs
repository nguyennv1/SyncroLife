namespace SyncroLife.DTOs.Habit
{
    public class UpdateHabitDTO
    {
        public string HabitName { get; set; } = null!;

        public string? HabitType { get; set; }

        public bool? IsPositive { get; set; }

        public int? FrequencyCount { get; set; }

        public string? FrequencyUnit { get; set; }

        public DateOnly? StartDate { get; set; }
    }
}