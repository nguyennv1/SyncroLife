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

        public async Task<List<Schedule>> GetSchedulesByDateAsync(Guid userId, DateTime date, int? timezoneOffset = null)
        {
            DateTime dayStartUtc = DateTime.SpecifyKind(date.Date, DateTimeKind.Utc);
            if (timezoneOffset.HasValue)
            {
                dayStartUtc = DateTime.SpecifyKind(date.Date.AddMinutes(-timezoneOffset.Value), DateTimeKind.Utc);
            }
            DateTime dayEndUtc = dayStartUtc.AddDays(1);

            return await _context.Schedules
                .Include(x => x.Type)
                .Where(x =>
                    x.UserId == userId &&
                    x.IsDeleted != true &&
                    x.StartTime >= dayStartUtc &&
                    x.StartTime < dayEndUtc)
                .OrderBy(x => x.StartTime)
                .ToListAsync();
        }

        public async Task<List<Schedule>> GetTodaySchedulesAsync(Guid userId, int? timezoneOffset = null)
        {
            DateTime localToday = DateTime.UtcNow;
            if (timezoneOffset.HasValue)
            {
                localToday = DateTime.UtcNow.AddMinutes(timezoneOffset.Value);
            }

            var todayDate = localToday.Date;

            DateTime dayStartUtc = DateTime.SpecifyKind(todayDate, DateTimeKind.Utc);
            if (timezoneOffset.HasValue)
            {
                dayStartUtc = DateTime.SpecifyKind(todayDate.AddMinutes(-timezoneOffset.Value), DateTimeKind.Utc);
            }
            DateTime dayEndUtc = dayStartUtc.AddDays(1);

            return await _context.Schedules
                .Include(x => x.Type)
                .Where(x =>
                    x.UserId == userId &&
                    x.IsDeleted != true &&
                    x.StartTime >= dayStartUtc &&
                    x.StartTime < dayEndUtc)
                .OrderBy(x => x.StartTime)
                .ToListAsync();
        }

        public async Task UpdateAsync(Schedule schedule)
        {
            _context.Schedules.Update(schedule);
            await _context.SaveChangesAsync();
        }

        public async Task<Schedule?> GetByGoogleEventIdAsync(Guid userId, string googleEventId)
        {
            return await _context.Schedules
                .FirstOrDefaultAsync(x =>
                    x.UserId == userId &&
                    x.GoogleEventId == googleEventId &&
                    x.IsDeleted != true);
        }
    }
}
