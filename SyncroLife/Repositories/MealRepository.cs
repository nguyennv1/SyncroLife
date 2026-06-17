using Microsoft.EntityFrameworkCore;
using SyncroLife.Data;
using SyncroLife.Interfaces.Repositories;
using SyncroLife.Models;

namespace SyncroLife.Repositories
{
    public class MealRepository : IMealRepository
    {
        private readonly SyncroLifeDbContext _context;

        public MealRepository(
            SyncroLifeDbContext context)
        {
            _context = context;
        }

        public async Task<List<Meal>> GetAllAsync()
        {
            return await _context.Meals
                .Where(x =>
                    x.IsDeleted != true)
                .ToListAsync();
        }
    }
}