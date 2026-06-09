using Microsoft.EntityFrameworkCore;
using SyncroLife.Data;
using SyncroLife.Interfaces.Repositories;
using SyncroLife.Models;

namespace SyncroLife.Repositories
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly SyncroLifeDbContext _context;

        public NotificationRepository(
            SyncroLifeDbContext context)
        {
            _context = context;
        }

        public async Task<List<Notification>> GetByUserIdAsync(Guid userId)
        {
            return await _context.Notifications
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Notification>> GetUnreadByUserIdAsync(Guid userId)
        {
            return await _context.Notifications
                .Where(x =>
                    x.UserId == userId &&
                    x.IsRead != true)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
        }

        public async Task<Notification?> GetByIdAsync(Guid notificationId)
        {
            return await _context.Notifications
                .FirstOrDefaultAsync(x =>
                    x.NotificationId == notificationId);
        }

        public async Task<int> GetUnreadCountAsync(Guid userId)
        {
            return await _context.Notifications
                .CountAsync(x =>
                    x.UserId == userId &&
                    x.IsRead != true);
        }

        public async Task UpdateAsync(Notification notification)
        {
            _context.Notifications.Update(notification);
            await _context.SaveChangesAsync();
        }

        public async Task AddAsync(Notification notification)
        {
            await _context.Notifications.AddAsync(notification);

            await _context.SaveChangesAsync();
        }
    }
}