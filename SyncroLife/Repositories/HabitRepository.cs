using Microsoft.EntityFrameworkCore;
using SyncroLife.Data;
using SyncroLife.Interfaces.Repositories;
using SyncroLife.Models;

namespace SyncroLife.Repositories
{
    public class HabitRepository : IHabitRepository
    {
        private readonly SyncroLifeDbContext _context;

        public HabitRepository(SyncroLifeDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Habit habit)
        {
            await _context.Habits.AddAsync(habit);
            await _context.SaveChangesAsync();
        }

        public async Task<Habit?> GetByIdAsync(Guid habitId)
        {
            return await _context.Habits
                .FirstOrDefaultAsync(x =>
                    x.HabitId == habitId &&
                    x.IsDeleted == false);
        }

        public async Task<List<Habit>> GetByUserIdAsync(Guid userId)
        {
            return await _context.Habits
                .Where(x =>
                    x.UserId == userId &&
                    x.IsDeleted == false)
                .ToListAsync();
        }

        public async Task UpdateAsync(Habit habit)
        {
            _context.Habits.Update(habit);
            await _context.SaveChangesAsync();
        }
    }
}