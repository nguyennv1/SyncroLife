using Microsoft.EntityFrameworkCore;
using SyncroLife.Data;
using SyncroLife.Interfaces.Repositories;
using SyncroLife.Models;

namespace SyncroLife.Repositories
{
    public class ReminderRepository : IReminderRepository
    {
        private readonly SyncroLifeDbContext _context;

        public ReminderRepository(SyncroLifeDbContext context)
        {
            _context = context;
        }

        public async Task<Reminder?> GetByIdAsync(Guid reminderId)
        {
            return await _context.Reminders
                .Include(r => r.Schedule)
                .FirstOrDefaultAsync(r =>
                    r.ReminderId == reminderId);
        }

        public async Task<List<Reminder>> GetByUserIdAsync(Guid userId)
        {
            return await _context.Reminders
                .Include(r => r.Schedule)
                .Where(r => r.Schedule.UserId == userId)
                .OrderBy(r => r.RemindAt)
                .ToListAsync();
        }

        public async Task AddAsync(Reminder reminder)
        {
            await _context.Reminders.AddAsync(reminder);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Reminder reminder)
        {
            _context.Reminders.Update(reminder);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Reminder reminder)
        {
            _context.Reminders.Remove(reminder);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ExistsAsync(Guid reminderId)
        {
            return await _context.Reminders
                .AnyAsync(r => r.ReminderId == reminderId);
        }

        public async Task<List<Reminder>> GetPendingRemindersAsync(DateTime utcNow)
        {
            return await _context.Reminders
                .Include(r => r.Schedule)
                .Where(r =>r.IsSent == false && r.RemindAt <= utcNow)
                .OrderBy(r => r.RemindAt)
                .ToListAsync();
        }
    }
}