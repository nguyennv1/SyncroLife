using SyncroLife.Models;

namespace SyncroLife.Interfaces.Repositories
{
    public interface IScheduleRepository
    {
            Task AddAsync(Schedule schedule);

            Task<Schedule?> GetByIdAsync(Guid scheduleId);

            Task<List<Schedule>> GetByUserIdAsync(Guid userId);

            Task<List<Schedule>> GetTodaySchedulesAsync(Guid userId);

            Task<List<Schedule>> GetSchedulesByDateAsync(Guid userId, DateTime date);

            Task UpdateAsync(Schedule schedule);
    }
}
