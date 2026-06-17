using Microsoft.EntityFrameworkCore;
using SyncroLife.Data;
using SyncroLife.Interfaces.Repositories;
using SyncroLife.Models;

namespace SyncroLife.Repositories
{
    public class UserDietaryPreferenceRepository : IUserDietaryPreferenceRepository
    {
        private readonly SyncroLifeDbContext _context;

        public UserDietaryPreferenceRepository(
            SyncroLifeDbContext context)
        {
            _context = context;
        }

        public async Task<List<DietaryPreference>>
            GetPreferencesByUserIdAsync(Guid userId)
        {
            return await _context
                .UserDietaryPreferences
                .Where(x =>
                    x.UserId == userId)
                .Include(x =>
                    x.Preference)
                .Select(x =>
                    x.Preference)
                .Where(x =>
                    x.IsDeleted != true &&
                    x.IsActive == true)
                .ToListAsync();
        }
    }
}