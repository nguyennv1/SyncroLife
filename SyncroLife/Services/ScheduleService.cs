using SyncroLife.DTOs.Schedule;
using SyncroLife.Interfaces.Repositories;
using SyncroLife.Interfaces.Services;
using SyncroLife.Models;

namespace SyncroLife.Services
{
    public class ScheduleService : IScheduleService
    {
        private readonly IScheduleRepository _scheduleRepository;
        private readonly IScheduleTypeRepository _scheduleTypeRepository;

        public ScheduleService(
            IScheduleRepository scheduleRepository,
            IScheduleTypeRepository scheduleTypeRepository)
        {
            _scheduleRepository = scheduleRepository;
            _scheduleTypeRepository = scheduleTypeRepository;
        }

        public async Task<ScheduleResponseDTO> CreateScheduleAsync(Guid userId, CreateScheduleDTO request)
        {
            if (string.IsNullOrWhiteSpace(request.Title))
            {
                throw new Exception("Title is required.");
            }

            if (request.EndTime <= request.StartTime)
            {
                throw new Exception("End time must be greater than start time.");
            }

            var scheduleType = await _scheduleTypeRepository.GetByIdAsync(request.TypeId);

            if (scheduleType == null)
            {
                throw new Exception("Schedule type not found.");
            }

            var schedule = new Schedule
            {
                ScheduleId = Guid.NewGuid(),
                UserId = userId,
                TypeId = request.TypeId,
                Title = request.Title,
                StartTime = request.StartTime.ToUniversalTime(),
                EndTime = request.EndTime.ToUniversalTime(),
                Description = request.Description,
                IsCompleted = false,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow
            };

            await _scheduleRepository.AddAsync(schedule);

            return new ScheduleResponseDTO
            {
                ScheduleId = schedule.ScheduleId,
                TypeId = schedule.TypeId,
                TypeName = scheduleType.TypeName,
                EnergyLevel = scheduleType.EnergyLevel,
                Title = schedule.Title,
                StartTime = schedule.StartTime,
                EndTime = schedule.EndTime,
                Description = schedule.Description,
                IsCompleted = schedule.IsCompleted ?? false,
                CreatedAt = schedule.CreatedAt
            };
        }

        public async Task<List<ScheduleResponseDTO>> GetSchedulesByUserAsync(Guid userId)
        {
            var schedules = await _scheduleRepository.GetByUserIdAsync(userId);

            return schedules.Select(x => new ScheduleResponseDTO
            {
                ScheduleId = x.ScheduleId,
                TypeId = x.TypeId,
                TypeName = x.Type.TypeName,
                EnergyLevel = x.Type.EnergyLevel,
                Title = x.Title,
                StartTime = x.StartTime,
                EndTime = x.EndTime,
                Description = x.Description,
                IsCompleted = x.IsCompleted ?? false,
                CreatedAt = x.CreatedAt
            }).ToList();
        }

        public async Task<ScheduleResponseDTO?> GetScheduleByIdAsync(Guid userId, Guid scheduleId)
        {
            var schedule = await _scheduleRepository.GetByIdAsync(scheduleId);

            if (schedule == null)
            {
                throw new Exception("Schedule not found.");
            }

            if (schedule.UserId != userId)
            {
                throw new Exception("You are not allowed to access this schedule.");
            }

            return new ScheduleResponseDTO
            {
                ScheduleId = schedule.ScheduleId,
                TypeId = schedule.TypeId,
                TypeName = schedule.Type.TypeName,
                EnergyLevel = schedule.Type.EnergyLevel,
                Title = schedule.Title,
                StartTime = schedule.StartTime,
                EndTime = schedule.EndTime,
                Description = schedule.Description,
                IsCompleted = schedule.IsCompleted ?? false,
                CreatedAt = schedule.CreatedAt
            };
        }

        public async Task<ScheduleResponseDTO> UpdateScheduleAsync(Guid userId, Guid scheduleId, UpdateScheduleDTO request)
        {
            if (string.IsNullOrWhiteSpace(request.Title))
            {
                throw new Exception("Title is required.");
            }

            if (request.EndTime <= request.StartTime)
            {
                throw new Exception("End time must be greater than start time.");
            }

            var schedule = await _scheduleRepository.GetByIdAsync(scheduleId);

            if (schedule == null)
            {
                throw new Exception("Schedule not found.");
            }

            if (schedule.UserId != userId)
            {
                throw new Exception("You are not allowed to access this schedule.");
            }

            var scheduleType = await _scheduleTypeRepository.GetByIdAsync(request.TypeId);

            if (scheduleType == null)
            {
                throw new Exception("Schedule type not found.");
            }

            schedule.TypeId = request.TypeId;
            schedule.Title = request.Title;
            schedule.StartTime = request.StartTime.ToUniversalTime();
            schedule.EndTime = request.EndTime.ToUniversalTime();
            schedule.Description = request.Description;
            schedule.IsCompleted = request.IsCompleted;
            schedule.UpdatedAt = DateTime.UtcNow;

            await _scheduleRepository.UpdateAsync(schedule);

            return new ScheduleResponseDTO
            {
                ScheduleId = schedule.ScheduleId,
                TypeId = schedule.TypeId,
                TypeName = scheduleType.TypeName,
                EnergyLevel = scheduleType.EnergyLevel,
                Title = schedule.Title,
                StartTime = schedule.StartTime,
                EndTime = schedule.EndTime,
                Description = schedule.Description,
                IsCompleted = schedule.IsCompleted ?? false,
                CreatedAt = schedule.CreatedAt
            };
        }

        public async Task<bool> DeleteScheduleAsync(Guid userId, Guid scheduleId)
        {
            var schedule = await _scheduleRepository.GetByIdAsync(scheduleId);

            if (schedule == null)
            {
                throw new Exception("Schedule not found.");
            }

            if (schedule.UserId != userId)
            {
                throw new Exception("You are not allowed to access this schedule.");
            }

            schedule.IsDeleted = true;
            schedule.DeletedAt = DateTime.UtcNow;

            await _scheduleRepository.UpdateAsync(schedule);

            return true;
        }

        public async Task<List<ScheduleResponseDTO>> GetTodaySchedulesAsync(Guid userId)
        {
            var schedules = await _scheduleRepository.GetTodaySchedulesAsync(userId);

            return schedules.Select(x => new ScheduleResponseDTO
            {
                ScheduleId = x.ScheduleId,
                TypeId = x.TypeId,
                TypeName = x.Type.TypeName,
                EnergyLevel = x.Type.EnergyLevel,
                Title = x.Title,
                StartTime = x.StartTime,
                EndTime = x.EndTime,
                Description = x.Description,
                IsCompleted = x.IsCompleted ?? false,
                CreatedAt = x.CreatedAt
            }).ToList();
        }

        public async Task<List<ScheduleResponseDTO>> GetSchedulesByDateAsync(Guid userId, DateTime date)
        {
            var schedules = await _scheduleRepository.GetSchedulesByDateAsync(userId, date);

            return schedules.Select(x => new ScheduleResponseDTO
            {
                ScheduleId = x.ScheduleId,
                TypeId = x.TypeId,
                TypeName = x.Type.TypeName,
                EnergyLevel = x.Type.EnergyLevel,
                Title = x.Title,
                StartTime = x.StartTime,
                EndTime = x.EndTime,
                Description = x.Description,
                IsCompleted = x.IsCompleted ?? false,
                CreatedAt = x.CreatedAt
            }).ToList();
        }
    }
}