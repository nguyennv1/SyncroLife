using Microsoft.EntityFrameworkCore;
using SyncroLife.Data;
using SyncroLife.Interfaces.Repositories;
using SyncroLife.Models;

namespace SyncroLife.Repositories
{
    public class AiRecommendationRepository : IAiRecommendationRepository
    {
        private readonly SyncroLifeDbContext _context;

        public AiRecommendationRepository(
            SyncroLifeDbContext context)
        {
            _context = context;
        }
        public async Task AddAsync(AiRecommendation recommendation)
        {
            await _context.AiRecommendations.AddAsync(recommendation);

            await _context.SaveChangesAsync();
        }

        

        public async Task<AiRecommendation?> GetByIdAsync(Guid recommendationId)
        {
            return await _context.AiRecommendations
                .FirstOrDefaultAsync(x => x.RecommendationId == recommendationId);
        }

        public async Task<List<AiRecommendation>> GetByUserIdAsync(Guid userId)
        {
            return await _context.AiRecommendations
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
        }

        public async Task<AiRecommendation?> GetLatestAsync(Guid userId)
        {
            return await _context.AiRecommendations
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefaultAsync();
        }

        public async Task UpdateAsync(AiRecommendation recommendation)
        {
            _context.AiRecommendations.Update(recommendation);

            await _context.SaveChangesAsync();
        }

        public async Task<bool> ExistsRecentAsync(Guid userId, string recommendationType, Guid? scheduleId, Guid? goalId, Guid? habitId, Guid? mealId, Guid? preferenceId)
        {
            var yesterday = DateTime.UtcNow.AddHours(-24);

            return await _context.AiRecommendations.AnyAsync(x =>
                x.UserId == userId &&
                x.RecommendationType == recommendationType &&
                x.ScheduleId == scheduleId &&
                x.GoalId == goalId &&
                x.HabitId == habitId &&
                x.MealId == mealId &&
                x.PreferenceId == preferenceId &&
                x.CreatedAt >= yesterday);
        }
    }
}
