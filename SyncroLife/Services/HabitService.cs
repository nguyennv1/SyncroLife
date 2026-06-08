using SyncroLife.DTOs.Habit;
using SyncroLife.Enums;
using SyncroLife.Interfaces.Repositories;
using SyncroLife.Interfaces.Services;
using SyncroLife.Models;

namespace SyncroLife.Services
{
    public class HabitService : IHabitService
    {
        private readonly IHabitRepository _habitRepository;
        public HabitService(IHabitRepository habitRepository)
        {
            _habitRepository = habitRepository;
        }

        public async Task<HabitResponseDTO> CreateHabitAsync(Guid userId, CreateHabitDTO request)
        {
            if (string.IsNullOrWhiteSpace(request.HabitName))
            {
                throw new Exception("Habit name is required.");
            }

            if (request.FrequencyCount.HasValue &&
                request.FrequencyCount <= 0)
            {
                throw new Exception(
                    "Frequency count must be greater than 0.");
            }

            if (!string.IsNullOrWhiteSpace(request.FrequencyUnit))
            {
                bool isValid =
                    Enum.TryParse<FrequencyUnit>(
                        request.FrequencyUnit,
                        true,
                        out _);

                if (!isValid)
                {
                    throw new Exception(
                        "Invalid frequency unit.");
                }
            }

            var habit = new Habit
            {
                HabitId = Guid.NewGuid(),
                UserId = userId,

                HabitName = request.HabitName,
                HabitType = request.HabitType,

                IsPositive = request.IsPositive ?? true,

                FrequencyCount = request.FrequencyCount,
                FrequencyUnit = request.FrequencyUnit,

                StartDate = request.StartDate,

                IsDeleted = false,

                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _habitRepository.AddAsync(habit);

            return new HabitResponseDTO
            {
                HabitId = habit.HabitId,
                HabitName = habit.HabitName,
                HabitType = habit.HabitType,
                IsPositive = habit.IsPositive,
                FrequencyCount = habit.FrequencyCount,
                FrequencyUnit = habit.FrequencyUnit,
                StartDate = habit.StartDate,
                CreatedAt = habit.CreatedAt
            };
        }

        

        public async Task<HabitResponseDTO> GetHabitByIdAsync(Guid userId, Guid habitId)
        {
            var habit =
                await _habitRepository.GetByIdAsync(habitId);

            if (habit == null)
            {
                throw new Exception("Habit not found.");
            }

            return new HabitResponseDTO
            {
                HabitId = habit.HabitId,
                HabitName = habit.HabitName,
                HabitType = habit.HabitType,
                IsPositive = habit.IsPositive,
                FrequencyCount = habit.FrequencyCount,
                FrequencyUnit = habit.FrequencyUnit,
                StartDate = habit.StartDate,
                CreatedAt = habit.CreatedAt
            };
        }

        public async Task<List<HabitResponseDTO>> GetHabitsByUserAsync(Guid userId)
        {
            var habits =
                await _habitRepository.GetByUserIdAsync(userId);

            return habits.Select(h => new HabitResponseDTO
            {
                HabitId = h.HabitId,
                HabitName = h.HabitName,
                HabitType = h.HabitType,
                IsPositive = h.IsPositive,
                FrequencyCount = h.FrequencyCount,
                FrequencyUnit = h.FrequencyUnit,
                StartDate = h.StartDate,
                CreatedAt = h.CreatedAt
            }).ToList();
        }

        public async Task<HabitResponseDTO> UpdateHabitAsync(Guid userId, Guid habitId, UpdateHabitDTO request)
        {
            var habit =
                await _habitRepository.GetByIdAsync(habitId);

            if (habit == null)
            {
                throw new Exception("Habit not found.");
            }

            if (string.IsNullOrWhiteSpace(request.HabitName))
            {
                throw new Exception("Habit name is required.");
            }

            if (request.FrequencyCount.HasValue &&
                request.FrequencyCount <= 0)
            {
                throw new Exception(
                    "Frequency count must be greater than 0.");
            }

            if (!string.IsNullOrWhiteSpace(request.FrequencyUnit))
            {
                bool isValid =
                    Enum.TryParse<FrequencyUnit>(
                        request.FrequencyUnit,
                        true,
                        out _);

                if (!isValid)
                {
                    throw new Exception(
                        "Invalid frequency unit.");
                }
            }

            habit.HabitName = request.HabitName;
            habit.HabitType = request.HabitType;
            habit.IsPositive = request.IsPositive;

            habit.FrequencyCount = request.FrequencyCount;
            habit.FrequencyUnit = request.FrequencyUnit;

            habit.StartDate = request.StartDate;

            habit.UpdatedAt = DateTime.UtcNow;

            await _habitRepository.UpdateAsync(habit);

            return new HabitResponseDTO
            {
                HabitId = habit.HabitId,
                HabitName = habit.HabitName,
                HabitType = habit.HabitType,
                IsPositive = habit.IsPositive,
                FrequencyCount = habit.FrequencyCount,
                FrequencyUnit = habit.FrequencyUnit,
                StartDate = habit.StartDate,
                CreatedAt = habit.CreatedAt
            };
        }
        public async Task<bool> DeleteHabitAsync(Guid userId, Guid habitId)
        {
            var habit =
                await _habitRepository.GetByIdAsync(habitId);

            if (habit == null)
            {
                throw new Exception("Habit not found.");
            }

            habit.IsDeleted = true;
            habit.DeletedAt = DateTime.UtcNow;
            habit.UpdatedAt = DateTime.UtcNow;

            await _habitRepository.UpdateAsync(habit);

            return true;
        }
    }
}
