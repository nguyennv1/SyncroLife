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
    }
}
