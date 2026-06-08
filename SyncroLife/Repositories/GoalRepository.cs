using Microsoft.EntityFrameworkCore;
using SyncroLife.Data;
using SyncroLife.Interfaces.Repositories;
using SyncroLife.Models;

namespace SyncroLife.Repositories
{
    public class GoalRepository : IGoalRepository
    {
        private readonly SyncroLifeDbContext _context;

        public GoalRepository(SyncroLifeDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Goal goal)
        {
            await _context.Goals.AddAsync(goal);
            await _context.SaveChangesAsync();
        }

        public async Task<Goal?> GetByIdAsync(Guid goalId)
        {
            return await _context.Goals
                .FirstOrDefaultAsync(x =>x.GoalId == goalId && x.IsDeleted == false);
        }

        public async Task<List<Goal>> GetByUserIdAsync(Guid userId)
        {
            return await _context.Goals
                 .Where(x => x.UserId == userId && x.IsDeleted == false)
                 .ToListAsync();
        }

        public async Task UpdateAsync(Goal goal)
        {
            _context.Goals.Update(goal);
            await _context.SaveChangesAsync();
        }
    }
}
