using SyncroLife.AIRecommendation;
using SyncroLife.DTOs.Recommendation;
using SyncroLife.Interfaces.Repositories;
using SyncroLife.Interfaces.Services;
using SyncroLife.Models;

namespace SyncroLife.Services
{
    public class RecommendationService : IRecommendationService
    {
        private readonly IUserRepository _userRepository;

        private readonly IScheduleRepository _scheduleRepository;

        private readonly IHabitRepository _habitRepository;

        private readonly IGoalRepository _goalRepository;
        private readonly INotificationService _notificationService;

        private readonly IAiRecommendationRepository _recommendationRepository;

        private readonly IRecommendationEngine _recommendationEngine;
        private readonly IMealRepository _mealRepository;

        private readonly IUserDietaryPreferenceRepository _preferenceRepository;
        public RecommendationService(IUserRepository userRepository, IScheduleRepository scheduleRepository, IHabitRepository habitRepository,
                IGoalRepository goalRepository, IAiRecommendationRepository recommendationRepository, IRecommendationEngine recommendationEngine,
                INotificationService notificationService, IMealRepository mealRepository, IUserDietaryPreferenceRepository preferenceRepository)
        {
            _userRepository = userRepository;
            _scheduleRepository = scheduleRepository;
            _habitRepository = habitRepository;
            _goalRepository = goalRepository;
            _recommendationRepository = recommendationRepository;
            _recommendationEngine = recommendationEngine;
            _notificationService = notificationService;
            _mealRepository = mealRepository;
            _preferenceRepository = preferenceRepository;
        }
        public async Task<List<RecommendationResponseDTO>> GenerateForUserAsync(Guid userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);

            if (user == null)
            {
                throw new Exception("User not found.");
            }

            var schedules = await _scheduleRepository.GetByUserIdAsync(userId);
            var habits = await _habitRepository.GetByUserIdAsync(userId);
            var goals = await _goalRepository.GetByUserIdAsync(userId);
            var meals = await _mealRepository.GetAllAsync();
            var preferences = await _preferenceRepository.GetPreferencesByUserIdAsync(userId);

            var context = new RecommendationContext
            {
                User = user,
                Schedules = schedules,
                Habits = habits,
                Goals = goals,
                Meals = meals,
                Preferences = preferences,
                CurrentTime = DateTime.UtcNow
            };

            var results = await _recommendationEngine.GenerateAsync(context);

            var recommendations = new List<AiRecommendation>();

            Console.WriteLine($"Recommendation Job Started - {DateTime.Now}");
            foreach (var result in results)
            {
                var exists = await _recommendationRepository.ExistsRecentAsync(
                    userId,
                    result.RecommendationType,
                    result.ScheduleId,
                    result.GoalId,
                    result.HabitId,
                    result.MealId,
                    result.PreferenceId);

                if (exists)
                {
                    continue;
                }

                var recommendation =
                    new AiRecommendation
                    {
                        RecommendationId = Guid.NewGuid(),
                        UserId = userId,
                        ScheduleId = result.ScheduleId,
                        HabitId = result.HabitId,
                        GoalId = result.GoalId,
                        MealId = result.MealId,
                        PreferenceId = result.PreferenceId,
                        RecommendationType = result.RecommendationType,
                        Reasoning = result.Reasoning,
                        Score = result.Score,
                        CreatedAt = DateTime.UtcNow
                    };

                await _recommendationRepository
                    .AddAsync(recommendation);

                await _notificationService
                    .CreateRecommendationNotificationAsync(
                        recommendation);

                recommendations.Add(recommendation);
            }
            Console.WriteLine($"Recommendation Job Finished - {DateTime.Now}");

            return recommendations.Select(x => new RecommendationResponseDTO
                {
                    RecommendationId = x.RecommendationId,
                    RecommendationType = x.RecommendationType,
                    Reasoning = x.Reasoning,
                    Score = x.Score,
                    ScheduleId = x.ScheduleId,
                    HabitId = x.HabitId,
                    GoalId = x.GoalId,
                    MealId = x.MealId,
                    PreferenceId = x.PreferenceId,
                    UserFeedback = x.UserFeedback,
                    CreatedAt = x.CreatedAt
                }).ToList();

}


        public async Task<List<RecommendationResponseDTO>> GetByUserAsync(Guid userId)
        {
            var recommendations =
        await _recommendationRepository.GetByUserIdAsync(userId);

            return recommendations.Select(x => new RecommendationResponseDTO
            {
                RecommendationId = x.RecommendationId,
                RecommendationType = x.RecommendationType,
                Reasoning = x.Reasoning,
                Score = x.Score,
                ScheduleId = x.ScheduleId,
                HabitId = x.HabitId,
                GoalId = x.GoalId,
                MealId = x.MealId,
                PreferenceId = x.PreferenceId,
                UserFeedback = x.UserFeedback,
                CreatedAt = x.CreatedAt
            }).ToList();
        }

        public async Task<RecommendationResponseDTO?> GetLatestAsync(Guid userId)
        {
            var recommendation =
        await _recommendationRepository.GetLatestAsync(userId);

            if (recommendation == null)
            {
                return null;
            }

            return new RecommendationResponseDTO
            {
                RecommendationId = recommendation.RecommendationId,
                RecommendationType = recommendation.RecommendationType,
                Reasoning = recommendation.Reasoning,
                Score = recommendation.Score,
                ScheduleId = recommendation.ScheduleId,
                HabitId = recommendation.HabitId,
                GoalId = recommendation.GoalId,
                MealId = recommendation.MealId,
                PreferenceId = recommendation.PreferenceId,
                UserFeedback = recommendation.UserFeedback,
                CreatedAt = recommendation.CreatedAt
            };
        }

        public async Task UpdateFeedbackAsync(Guid userId, Guid recommendationId, string feedback)
        {
            var recommendation =
        await _recommendationRepository.GetByIdAsync(recommendationId);

            if (recommendation == null)
            {
                throw new Exception("Recommendation not found.");
            }

            if (recommendation.UserId != userId)
            {
                throw new Exception("You are not allowed.");
            }

            recommendation.UserFeedback = feedback;
            recommendation.UpdatedAt = DateTime.UtcNow;

            await _recommendationRepository.UpdateAsync(recommendation);
        }
    }

}
