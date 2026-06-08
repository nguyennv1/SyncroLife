using SyncroLife.DTOs.ScheduleType;

namespace SyncroLife.Interfaces.Services
{
    public interface IScheduleTypeService
    {
        Task<List<ScheduleTypeResponseDTO>> GetAllAsync();
    }
}
