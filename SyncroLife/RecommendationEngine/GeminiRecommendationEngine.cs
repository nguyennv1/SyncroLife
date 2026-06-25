using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Options;
using SyncroLife.DTOs.FoodAnalysis;
using SyncroLife.Helpers;
using SyncroLife.Models;

namespace SyncroLife.AIRecommendation
{
    public class GeminiRecommendationEngine : IRecommendationEngine
    {
        private readonly HttpClient _httpClient;
        private readonly GeminiSettings _settings;

        public GeminiRecommendationEngine(
            HttpClient httpClient,
            IOptions<GeminiSettings> options)
        {
            _httpClient = httpClient;
            _settings = options.Value;
        }

        public async Task<List<RecommendationResult>> GenerateAsync(RecommendationContext context)
        {
            var prompt = BuildPrompt(context);

            var requestBody = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new object[]
                        {
                            new
                            {
                                text = prompt
                            }
                        }
                    }
                }
            };

            var url = $"https://generativelanguage.googleapis.com/v1beta/models/{_settings.Model}:generateContent?key={_settings.ApiKey}";

            try
            {
                var response = await _httpClient.PostAsJsonAsync(url, requestBody);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Gemini API Error: {responseContent}");
                    return GetFallbackRecommendations(context);
                }

                var geminiResponse = JsonSerializer.Deserialize<GeminiApiResponse>(responseContent,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                var jsonText = geminiResponse?.Candidates
                    .FirstOrDefault()?.Content.Parts
                    .FirstOrDefault()?.Text;

                jsonText = jsonText?
                    .Replace("```json", "")
                    .Replace("```", "")
                    .Trim();

                if (string.IsNullOrWhiteSpace(jsonText))
                {
                    Console.WriteLine("Gemini API returned an empty JSON text.");
                    return GetFallbackRecommendations(context);
                }

                var results = JsonSerializer.Deserialize<List<RecommendationResult>>(jsonText,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                if (results == null || !results.Any())
                {
                    return GetFallbackRecommendations(context);
                }

                // Ensure contextual IDs are correctly mapped/validated to prevent database constraint issues
                foreach (var r in results)
                {
                    if (r.ScheduleId.HasValue && !context.Schedules.Any(s => s.ScheduleId == r.ScheduleId.Value))
                    {
                        r.ScheduleId = null;
                    }
                    if (r.HabitId.HasValue && !context.Habits.Any(h => h.HabitId == r.HabitId.Value))
                    {
                        r.HabitId = null;
                    }
                    if (r.GoalId.HasValue && !context.Goals.Any(g => g.GoalId == r.GoalId.Value))
                    {
                        r.GoalId = null;
                    }
                    if (r.MealId.HasValue && !context.Meals.Any(m => m.MealId == r.MealId.Value))
                    {
                        r.MealId = null;
                    }
                    if (r.PreferenceId.HasValue && !context.Preferences.Any(p => p.PreferenceId == r.PreferenceId.Value))
                    {
                        r.PreferenceId = null;
                    }
                }

                return results;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in GeminiRecommendationEngine: {ex.Message}");
                return GetFallbackRecommendations(context);
            }
        }

        private string BuildPrompt(RecommendationContext context)
        {
            var vietnamTime = context.CurrentTime.AddHours(7); // Convert UTC to Vietnam Time (UTC+7)

            var schedulesData = context.Schedules
                .Where(s => s.IsDeleted != true)
                .Select(s => new
                {
                    s.ScheduleId,
                    s.Title,
                    StartTime = s.StartTime.AddHours(7), // Convert UTC start time to Vietnam Time (UTC+7)
                    EndTime = s.EndTime.AddHours(7),     // Convert UTC end time to Vietnam Time (UTC+7)
                    s.Description,
                    EnergyLevel = s.Type?.EnergyLevel
                }).ToList();

            var habitsData = context.Habits
                .Where(h => h.IsDeleted != true)
                .Select(h => new
                {
                    h.HabitId,
                    h.HabitName,
                    h.HabitType,
                    h.IsPositive,
                    h.FrequencyCount,
                    h.FrequencyUnit
                }).ToList();

            var goalsData = context.Goals
                .Where(g => g.IsDeleted != true)
                .Select(g => new
                {
                    g.GoalId,
                    g.GoalName,
                    g.GoalType,
                    g.TargetValue,
                    g.CurrentValue,
                    g.Unit,
                    g.Deadline,
                    g.ProgressPercentage
                }).ToList();

            var mealsData = context.Meals
                .Where(m => m.IsDeleted != true)
                .Select(m => new
                {
                    m.MealId,
                    m.Name,
                    m.Description,
                    m.Category,
                    m.Calories,
                    m.Protein,
                    m.Carbs,
                    m.Fats,
                    m.IsVegetarian,
                    m.IsVegan
                }).ToList();

            var preferencesData = context.Preferences
                .Select(p => new
                {
                    p.PreferenceId,
                    p.PreferenceName,
                    p.PreferenceType
                }).ToList();

            var prompt = $@"
You are the AI lifestyle and nutrition coach of Syncro Life.
Your task is to analyze the user's schedule, habits, goals, available meals, and dietary preferences to generate highly personalized, actionable advice.

Current local time (Vietnam Time, UTC+7): {vietnamTime:yyyy-MM-dd HH:mm:ss}
User Information:
- Name: {context.User.FullName}
- Email: {context.User.Email}
- Gender: {context.User.Gender ?? "Not specified"}
- Height: {context.User.Height ?? 0} cm
- Weight: {context.User.Weight ?? 0} kg
- Target Daily Calories: {context.User.TargetCalories ?? 0} kcal
- Allergies / Dietary Restrictions: {context.User.Allergies ?? "None"}

Today's/Upcoming Schedules (already in Vietnam Time, UTC+7):
{JsonSerializer.Serialize(schedulesData)}

Habits list:
{JsonSerializer.Serialize(habitsData)}

Goals list:
{JsonSerializer.Serialize(goalsData)}

Available Meals in Database:
{JsonSerializer.Serialize(mealsData)}

Dietary Preferences:
{JsonSerializer.Serialize(preferencesData)}

Requirements:
1. Keep the 'reasoning' 100% in English. Always begin the 'reasoning' text with a greeting containing the user's full name, e.g. 'Hello {context.User.FullName}! ' or 'Hi {context.User.FullName}! '.
2. The 'reasoning' must be very short and concise (under 2-3 sentences, about 1/3 of standard paragraph length, maximum 50 words). Reduce unnecessary details.
3. If referring to any time in the 'reasoning', ONLY use Vietnam Time (UTC+7) in 24-hour format (e.g. '18:00', not UTC/Z format). Note that the input schedules are already converted to Vietnam Time (UTC+7) for you.
4. CRITICAL ALLERGY RULE: The user is allergic to: {context.User.Allergies ?? "None"}. You MUST NOT recommend any meals from the database that contain these allergens. Analyze the name and description of the available meals and exclude any that contain the user's allergens.
5. If the user has a high-energy activity (like gym, workout, running) scheduled, suggest the best matching high-protein meal from the database.
6. If a goal deadline is near and progress is low, set the recommendation type to 'goal_warning'. If progress is high, set it to 'goal_progress'.
7. Map IDs (ScheduleId, HabitId, GoalId, MealId, PreferenceId) exactly matching the corresponding inputs. Use null if they are not related.

Return ONLY a valid JSON array of objects with this schema (no surrounding text or markdown, just the JSON array):
[
  {{
    ""recommendationType"": ""meal_suggestion | goal_progress | goal_warning | habit_coaching | lifestyle_coaching"",
    ""reasoning"": ""Concise advice in English..."",
    ""score"": 0.95,
    ""scheduleId"": ""guid or null"",
    ""habitId"": ""guid or null"",
    ""goalId"": ""guid or null"",
    ""mealId"": ""guid or null"",
    ""preferenceId"": ""guid or null""
  }}
]
";
            return prompt;
        }

        private List<RecommendationResult> GetFallbackRecommendations(RecommendationContext context)
        {
            // Simple rule-based fallback if Gemini fails or is rate-limited
            var recommendations = new List<RecommendationResult>();

            // Goal
            foreach (var goal in context.Goals.Where(g => g.IsDeleted != true))
            {
                if (goal.ProgressPercentage >= 80)
                {
                    recommendations.Add(new RecommendationResult
                    {
                        GoalId = goal.GoalId,
                        RecommendationType = "goal_progress",
                        Reasoning = $"You have completed {goal.ProgressPercentage}% of your goal '{goal.GoalName}'. Keep it up!",
                        Score = 0.95m
                    });
                }
            }

            // Habit
            foreach (var habit in context.Habits.Where(h => h.IsDeleted != true))
            {
                if (habit.IsPositive == true)
                {
                    recommendations.Add(new RecommendationResult
                    {
                        HabitId = habit.HabitId,
                        RecommendationType = "habit_coaching",
                        Reasoning = $"Keep up the good habit: '{habit.HabitName}'.",
                        Score = 0.80m
                    });
                }
            }

            // Meal
            var workoutSchedule = context.Schedules
                .FirstOrDefault(x => x.IsDeleted != true && x.Type?.EnergyLevel?.ToLower() == "high");

            if (workoutSchedule != null)
            {
                var highProteinMeal = context.Meals
                    .Where(m => m.IsDeleted != true)
                    .OrderByDescending(x => x.Protein)
                    .FirstOrDefault();

                if (highProteinMeal != null)
                {
                    recommendations.Add(new RecommendationResult
                    {
                        ScheduleId = workoutSchedule.ScheduleId,
                        MealId = highProteinMeal.MealId,
                        RecommendationType = "meal_suggestion",
                        Reasoning = $"You have a workout scheduled. '{highProteinMeal.Name}' is a great choice to fuel your body with protein.",
                        Score = 0.95m
                    });
                }
            }

            return recommendations;
        }
    }
}
