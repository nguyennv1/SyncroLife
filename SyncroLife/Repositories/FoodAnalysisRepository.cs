using SyncroLife.Data;
using SyncroLife.Interfaces.Repositories;
using SyncroLife.Models;
using Microsoft.EntityFrameworkCore;


namespace SyncroLife.Repositories
{
    public class FoodAnalysisRepository : IFoodAnalysisRepository
    {
        private readonly SyncroLifeDbContext _context;
        public FoodAnalysisRepository(SyncroLifeDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(FoodAnalysis analysis)
        {
            await _context.FoodAnalyses.AddAsync(analysis);

            await _context.SaveChangesAsync();
        }

        public async Task<FoodAnalysis?> GetByIdAsync(Guid analysisId)
        {
            return await _context.FoodAnalyses
            .FirstOrDefaultAsync(
                x => x.AnalysisId == analysisId);
        }

        public async Task<List<FoodAnalysis>> GetByUserIdAsync(Guid userId)
        {
            return await _context.FoodAnalyses
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
        }
    }
}
