using SyncroLife.Models;

namespace SyncroLife.AIRecommendation
{
    public class RuleBasedRecommendationEngine
        : IRecommendationEngine
    {
        public async Task<List<RecommendationResult>>
            GenerateAsync(
                RecommendationContext context)
        {
            var recommendations =
                new List<RecommendationResult>();

            GenerateGoalRecommendations(
                context,
                recommendations);

            GenerateHabitRecommendations(
                context,
                recommendations);

            GenerateMealRecommendations(
                context,
                recommendations);

            return recommendations;
        }

        private static void GenerateGoalRecommendations(
            RecommendationContext context,
            List<RecommendationResult> recommendations)
        {
            foreach (var goal in context.Goals)
            {
                if (goal.ProgressPercentage >= 80)
                {
                    recommendations.Add(
                        new RecommendationResult
                        {
                            GoalId = goal.GoalId,
                            RecommendationType =
                                "goal_progress",

                            Reasoning =
                                $"Bạn đã hoàn thành {goal.ProgressPercentage}% mục tiêu {goal.GoalName}. Hãy tiếp tục duy trì.",

                            Score = 0.95m
                        });
                }

                if (goal.Deadline.HasValue)
                {
                    var remainingDays =
                        goal.Deadline.Value
                            .ToDateTime(
                                TimeOnly.MinValue)
                        .Subtract(
                            context.CurrentTime)
                        .Days;

                    if (remainingDays <= 7 &&
                        goal.ProgressPercentage < 50)
                    {
                        recommendations.Add(
                            new RecommendationResult
                            {
                                GoalId = goal.GoalId,
                                RecommendationType =
                                    "goal_warning",

                                Reasoning =
                                    $"Mục tiêu {goal.GoalName} đang chậm tiến độ. Bạn nên tăng cường hoạt động trong tuần này.",

                                Score = 0.90m
                            });
                    }
                }
            }
        }

        private static void GenerateHabitRecommendations(
            RecommendationContext context,
            List<RecommendationResult> recommendations)
        {
            foreach (var habit in context.Habits)
            {
                if (habit.IsPositive == true)
                {
                    recommendations.Add(
                        new RecommendationResult
                        {
                            HabitId = habit.HabitId,

                            RecommendationType =
                                "habit_coaching",

                            Reasoning =
                                $"Hãy tiếp tục duy trì thói quen {habit.HabitName}.",

                            Score = 0.80m
                        });
                }
            }
        }

        private static void GenerateMealRecommendations(
            RecommendationContext context,
            List<RecommendationResult> recommendations)
        {
            var workoutSchedule =
                context.Schedules
                    .FirstOrDefault(x =>
                        x.Type.EnergyLevel != null &&
                        x.Type.EnergyLevel
                            .ToLower() == "high");

            if (workoutSchedule == null)
            {
                return;
            }

            var highProteinMeal =
                context.Meals
                    .OrderByDescending(x => x.Protein)
                    .FirstOrDefault();

            if (highProteinMeal == null)
            {
                return;
            }

            recommendations.Add(
                new RecommendationResult
                {
                    ScheduleId =
                        workoutSchedule.ScheduleId,

                    MealId =
                        highProteinMeal.MealId,

                    RecommendationType =
                        "meal_suggestion",

                    Reasoning =
                        $"Hôm nay bạn có buổi tập gym. Món {highProteinMeal.Name} phù hợp để bổ sung protein trước hoặc sau buổi tập.",

                    Score = 0.95m
                });
        }
    }
}