using SyncroLife.DTOs.Habit;

namespace SyncroLife.Interfaces.Services
{
    public interface IHabitService
    {
        Task<HabitResponseDTO> CreateHabitAsync(Guid userId,CreateHabitDTO request);

        Task<List<HabitResponseDTO>> GetHabitsByUserAsync(Guid userId);

        Task<HabitResponseDTO> GetHabitByIdAsync(Guid userId, Guid habitId);

        Task<HabitResponseDTO> UpdateHabitAsync(Guid userId, Guid habitId,UpdateHabitDTO request);

        Task<bool> DeleteHabitAsync(Guid userId, Guid habitId);
    }
}