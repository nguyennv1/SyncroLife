using SyncroLife.DTOs.FoodAnalysis;
using SyncroLife.Interfaces.Repositories;
using SyncroLife.Interfaces.Services;
using SyncroLife.Models;
using SyncroLife.Data;
using Microsoft.EntityFrameworkCore;

namespace SyncroLife.Services
{
    public class FoodAnalysisService : IFoodAnalysisService
    {
        private readonly IFoodAnalysisRepository _foodAnalysisRepository;
        private readonly IGeminiService _geminiService;
        private readonly SyncroLifeDbContext _context;

        public FoodAnalysisService(
            IFoodAnalysisRepository foodAnalysisRepository,
            IGeminiService geminiService,
            SyncroLifeDbContext context)
        {
            _foodAnalysisRepository = foodAnalysisRepository;
            _geminiService = geminiService;
            _context = context;
        }

        public async Task<AnalyzeFoodResponse> AnalyzeFoodAsync(Guid userId, IFormFile image)
        {
            if (image == null || image.Length == 0)
            {
                throw new Exception("Image is required.");
            }

            // 1. Check daily limit based on user subscription
            var subscription = await _context.UserSubscriptions
                .Include(s => s.Plan)
                .FirstOrDefaultAsync(s => s.UserId == userId && s.Status == "active");

            var todayDate = DateOnly.FromDateTime(DateTime.UtcNow);
            if (subscription != null && subscription.EndDate.HasValue && subscription.EndDate.Value < todayDate)
            {
                subscription.Status = "expired";
                subscription.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                subscription = null;
            }

            if (subscription == null)
            {
                var freePlan = await _context.SubscriptionPlans
                    .FirstOrDefaultAsync(p => p.PlanName.ToLower() == "free");

                subscription = new UserSubscription
                {
                    UserSubId = Guid.NewGuid(),
                    UserId = userId,
                    PlanId = freePlan?.PlanId ?? Guid.Parse("8a6c40ca-ef52-4dfb-a977-127c1d66d08b"),
                    StartDate = DateOnly.FromDateTime(DateTime.UtcNow),
                    Status = "active",
                    ScanCount = 0,
                    LastScanDate = null,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                await _context.UserSubscriptions.AddAsync(subscription);
                await _context.SaveChangesAsync();
            }

            var now = DateTime.UtcNow;
            if (subscription.ScanCount > 0)
            {
                var count = subscription.ScanCount.Value;
                var recentScans = await _context.FoodAnalyses
                    .Where(f => f.UserId == userId)
                    .OrderByDescending(f => f.CreatedAt)
                    .Take(count)
                    .ToListAsync();

                if (recentScans.Count == count)
                {
                    var oldestScanInCycle = recentScans.Last();
                    var cycleStart = oldestScanInCycle.CreatedAt ?? now;
                    if (now - cycleStart >= TimeSpan.FromHours(24))
                    {
                        subscription.ScanCount = 0;
                    }
                }
                else
                {
                    subscription.ScanCount = 0;
                }
            }
            else
            {
                subscription.ScanCount = 0;
            }

            int limit = 3;
            if (subscription.Plan != null)
            {
                var planName = subscription.Plan.PlanName.ToLower();
                if (planName == "plus")
                {
                    limit = 20;
                }
            }

            if (subscription.ScanCount >= limit)
            {
                throw new Exception($"You have reached your daily food analysis limit ({limit} scans/day for {subscription.Plan?.PlanName ?? "Free"} accounts). Please upgrade your account to Plus for higher limits, or try again tomorrow.");
            }

            using var memoryStream = new MemoryStream();

            await image.CopyToAsync(memoryStream);

            var imageBytes = memoryStream.ToArray();

            var geminiResult =
                await _geminiService.AnalyzeFoodAsync(imageBytes);

            // Increment count only after successful analysis
            subscription.ScanCount = (subscription.ScanCount ?? 0) + 1;
            subscription.LastScanDate = now;
            subscription.UpdatedAt = now;

            var analysis = new FoodAnalysis
            {
                AnalysisId = Guid.NewGuid(),

                UserId = userId,

                ImageUrl = null,

                DetectedMealName = geminiResult.FoodName,

                ConfidenceScore = Math.Clamp(geminiResult.Confidence, 0m, 1m),

                Calories = geminiResult.Calories,

                Protein = Math.Clamp(geminiResult.Protein, 0m, 999.99m),

                Carbs = Math.Clamp(geminiResult.Carbs, 0m, 999.99m),

                Fats = Math.Clamp(geminiResult.Fats, 0m, 999.99m),

                Fiber = Math.Clamp(geminiResult.Fiber, 0m, 999.99m),

                Sodium = Math.Clamp(geminiResult.Sodium, 0m, 999.99m),

                AnalysisResult = geminiResult.Description,

                AiNotes = geminiResult.AiNotes,

                CreatedAt = DateTime.UtcNow,

                UpdatedAt = DateTime.UtcNow
            };

            await _foodAnalysisRepository.AddAsync(analysis);
            await _context.SaveChangesAsync();

            return new AnalyzeFoodResponse
            {
                AnalysisId = analysis.AnalysisId,

                FoodName = geminiResult.FoodName,

                Confidence = geminiResult.Confidence,

                Calories = geminiResult.Calories,

                Protein = geminiResult.Protein,

                Carbs = geminiResult.Carbs,

                Fats = geminiResult.Fats,

                Fiber = geminiResult.Fiber,

                Sodium = geminiResult.Sodium,

                Description = geminiResult.Description,

                AiNotes = geminiResult.AiNotes,

                ImageUrl = analysis.ImageUrl,

                CreatedAt = analysis.CreatedAt ?? DateTime.UtcNow
            };
        }

        public async Task<List<AnalyzeFoodResponse>> GetHistoryAsync(Guid userId)
        {
            var subscription = await _context.UserSubscriptions
                .Include(s => s.Plan)
                .FirstOrDefaultAsync(s => s.UserId == userId && s.Status == "active");

            var todayDate = DateOnly.FromDateTime(DateTime.UtcNow);
            if (subscription != null && subscription.EndDate.HasValue && subscription.EndDate.Value < todayDate)
            {
                subscription.Status = "expired";
                subscription.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                subscription = null;
            }

            int loadCount = 5;
            if (subscription?.Plan != null && subscription.Plan.PlanName.ToLower() == "plus")
            {
                loadCount = 10;
            }

            var analyses = await _foodAnalysisRepository.GetByUserIdAsync(userId);
            return analyses.Take(loadCount).Select(x => new AnalyzeFoodResponse
            {
                AnalysisId = x.AnalysisId,
                FoodName = x.DetectedMealName ?? "Unknown Food",
                Confidence = x.ConfidenceScore ?? 0,
                Calories = x.Calories ?? 0,
                Protein = x.Protein ?? 0,
                Carbs = x.Carbs ?? 0,
                Fats = x.Fats ?? 0,
                Fiber = x.Fiber ?? 0,
                Sodium = x.Sodium ?? 0,
                Description = x.AnalysisResult ?? string.Empty,
                AiNotes = x.AiNotes ?? string.Empty,
                ImageUrl = x.ImageUrl,
                CreatedAt = x.CreatedAt ?? DateTime.UtcNow
            }).ToList();
        }
    }
}
