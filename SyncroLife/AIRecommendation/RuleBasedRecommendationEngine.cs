
namespace SyncroLife.AIRecommendation
{
    public class RuleBasedRecommendationEngine : IRecommendationEngine
    {
        public async Task<List<RecommendationResult>> GenerateAsync(RecommendationContext context)
        {
            var recommendations = new List<RecommendationResult>();

            var upcomingSchedules = context.Schedules
                .Where(x =>
                    x.StartTime > context.CurrentTime &&
                    x.StartTime <= context.CurrentTime.AddMinutes(30))
                .ToList();

            foreach (var schedule in upcomingSchedules)
            {
                recommendations.Add(new RecommendationResult
                {
                    RecommendationType = "schedule",
                    ScheduleId = schedule.ScheduleId,
                    Reasoning = $"Bạn có lịch {schedule.Title} sắp bắt đầu.",
                    Score = 0.9m
                });
            }

            return recommendations;
        }
    }
}
