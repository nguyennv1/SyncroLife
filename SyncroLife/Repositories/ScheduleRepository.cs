using Microsoft.EntityFrameworkCore;
using SyncroLife.Data;
using SyncroLife.Interfaces.Repositories;
using SyncroLife.Models;

namespace SyncroLife.Repositories
{
    public class ScheduleRepository : IScheduleRepository
    {
        private readonly SyncroLifeDbContext _context;

        public ScheduleRepository(
            SyncroLifeDbContext context)
        {
            _context = context;
        }
        public async Task AddAsync(Schedule schedule)
        {
            await _context.Schedules.AddAsync(schedule);
            await _context.SaveChangesAsync();
        }

        public async Task<Schedule?> GetByIdAsync(Guid scheduleId)
        {
            return await _context.Schedules
                .Include(x => x.Type)
                .FirstOrDefaultAsync(x =>
                    x.ScheduleId == scheduleId &&
                    x.IsDeleted != true);
        }

        public async Task<List<Schedule>> GetByUserIdAsync(Guid userId)
        {
            return await _context.Schedules
                .Include(x => x.Type)
                .Where(x =>
                    x.UserId == userId &&
                    x.IsDeleted != true)
                .OrderBy(x => x.StartTime)
                .ToListAsync();
        }

        public async Task<List<Schedule>> GetSchedulesByDateAsync(Guid userId, DateTime date)
        {
            return await _context.Schedules
                .Include(x => x.Type)
                .Where(x =>
                    x.UserId == userId &&
                    x.IsDeleted != true &&
                    x.StartTime.Date == date.Date)
                .OrderBy(x => x.StartTime)
                .ToListAsync();
        }

        public async Task<List<Schedule>> GetTodaySchedulesAsync(Guid userId)
        {
            var today = DateTime.UtcNow.Date;

            return await _context.Schedules
                .Include(x => x.Type)
                .Where(x =>
                    x.UserId == userId &&
                    x.IsDeleted != true &&
                    x.StartTime.Date == today)
                .OrderBy(x => x.StartTime)
                .ToListAsync();
        }

        public async Task UpdateAsync(Schedule schedule)
        {
            _context.Schedules.Update(schedule);
            await _context.SaveChangesAsync();
        }
    }
}
