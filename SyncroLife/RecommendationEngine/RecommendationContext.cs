using SyncroLife.Models;

namespace SyncroLife.AIRecommendation
{
    public class RecommendationContext
    {
        public User User { get; set; } = null!;

        public List<Schedule> Schedules { get; set; } = new();

        public List<Habit> Habits { get; set; } = new();

        public List<Goal> Goals { get; set; } = new();

        public List<Meal> Meals { get; set; } = new();

        public List<DietaryPreference> Preferences { get; set; } = new();

        public DateTime CurrentTime { get; set; }
    }
}