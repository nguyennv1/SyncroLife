using SyncroLife.Models;

namespace SyncroLife.Interfaces.Repositories
{
    public interface IScheduleTypeRepository
    {
        Task<List<ScheduleType>> GetAllAsync();
        Task<ScheduleType?> GetByIdAsync(Guid typeId);
    }
}
