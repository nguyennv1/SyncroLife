using SyncroLife.Data;
using SyncroLife.Interfaces.Repositories;
using SyncroLife.Models;
using Microsoft.EntityFrameworkCore;

namespace SyncroLife.Repositories
{
    public class UserDeviceTokenRepository : IUserDeviceTokenRepository
    {
        private readonly SyncroLifeDbContext _context;

        public UserDeviceTokenRepository(SyncroLifeDbContext context)
        {
            _context = context;
        }
        public async Task AddAsync(UserDeviceToken token)
        {
            await _context.UserDeviceTokens.AddAsync(token);
            await _context.SaveChangesAsync();
        }

        public async Task<UserDeviceToken?> GetByUserIdAsync(Guid userId)
        {
            return await _context.UserDeviceTokens
            .FirstOrDefaultAsync(x => x.UserId == userId);
        }

        public async Task UpdateAsync(UserDeviceToken token)
        {
            _context.UserDeviceTokens.Update(token);
            await _context.SaveChangesAsync();
        }
    }
}
