using SyncroLife.Models;

namespace SyncroLife.Interfaces.Repositories
{
    public interface IScheduleRepository
    {
            Task AddAsync(Schedule schedule);

            Task<Schedule?> GetByIdAsync(Guid scheduleId);

            Task<List<Schedule>> GetByUserIdAsync(Guid userId);

            Task<List<Schedule>> GetTodaySchedulesAsync(Guid userId, int? timezoneOffset = null);

            Task<List<Schedule>> GetSchedulesByDateAsync(Guid userId, DateTime date, int? timezoneOffset = null);

            Task UpdateAsync(Schedule schedule);
            Task<Schedule?> GetByGoogleEventIdAsync(Guid userId, string googleEventId);
    }
}
