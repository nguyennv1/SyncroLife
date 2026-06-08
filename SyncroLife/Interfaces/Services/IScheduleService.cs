using SyncroLife.DTOs.Schedule;

namespace SyncroLife.Interfaces.Services
{
    public interface IScheduleService
    {
        Task<ScheduleResponseDTO> CreateScheduleAsync(Guid userId, CreateScheduleDTO request);

        Task<List<ScheduleResponseDTO>> GetSchedulesByUserAsync(Guid userId);

        Task<ScheduleResponseDTO?> GetScheduleByIdAsync(Guid userId, Guid scheduleId);

        Task<ScheduleResponseDTO>UpdateScheduleAsync(Guid userId, Guid scheduleId, UpdateScheduleDTO request);

        Task<bool> DeleteScheduleAsync(Guid userId, Guid scheduleId);

        Task<List<ScheduleResponseDTO>> GetTodaySchedulesAsync(Guid userId);

        Task<List<ScheduleResponseDTO>> GetSchedulesByDateAsync(Guid userId, DateTime date);
    }
}