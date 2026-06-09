using SyncroLife.DTOs.Reminder;

namespace SyncroLife.Interfaces.Services
{
    public interface IReminderService
    {
        Task<ReminderResponseDTO> CreateAsync(Guid userId, CreateReminderDTO request);

        Task<List<ReminderResponseDTO>> GetMyRemindersAsync(Guid userId);

        Task<ReminderResponseDTO> GetByIdAsync(Guid userId, Guid reminderId);

        Task<ReminderResponseDTO> UpdateAsync(Guid userId, Guid reminderId, UpdateReminderDTO request);

        Task DeleteAsync(Guid userId, Guid reminderId);
    }
}