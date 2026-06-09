using SyncroLife.DTOs.Reminder;
using SyncroLife.Interfaces.Repositories;
using SyncroLife.Interfaces.Services;
using SyncroLife.Models;

namespace SyncroLife.Services
{
    public class ReminderService : IReminderService
    {
        private readonly IReminderRepository _reminderRepository;
        private readonly IScheduleRepository _scheduleRepository;

        public ReminderService(IReminderRepository reminderRepository, IScheduleRepository scheduleRepository)
        {
            _reminderRepository = reminderRepository;
            _scheduleRepository = scheduleRepository;
        }

        public async Task<ReminderResponseDTO> CreateAsync(Guid userId, CreateReminderDTO request)
        {
            var schedule = await _scheduleRepository
                .GetByIdAsync(request.ScheduleId);

            if (schedule == null)
            {
                throw new Exception("Schedule not found.");
            }

            if (schedule.UserId != userId)
            {
                throw new Exception(
                    "You are not allowed to access this schedule.");
            }

            if (request.RemindAt.ToUniversalTime()
                >= schedule.StartTime)
            {
                throw new Exception(
                    "Reminder time must be before schedule start time.");
            }

            var reminder = new Reminder
            {
                ReminderId = Guid.NewGuid(),
                ScheduleId = request.ScheduleId,
                RemindAt = request.RemindAt.ToUniversalTime(),
                ReminderType = request.ReminderType,
                IsSent = false,
                CreatedAt = DateTime.UtcNow
            };

            await _reminderRepository.AddAsync(reminder);

            return new ReminderResponseDTO
            {
                ReminderId = reminder.ReminderId,
                ScheduleId = schedule.ScheduleId,
                ScheduleTitle = schedule.Title,
                RemindAt = reminder.RemindAt,
                ReminderType = reminder.ReminderType ?? "",
                IsSent = reminder.IsSent ?? false
            };
        }

        public async Task<List<ReminderResponseDTO>> GetMyRemindersAsync(Guid userId)
        {
            var reminders = await _reminderRepository
                .GetByUserIdAsync(userId);

            return reminders.Select(x =>
                new ReminderResponseDTO
                {
                    ReminderId = x.ReminderId,
                    ScheduleId = x.ScheduleId,
                    ScheduleTitle = x.Schedule.Title,
                    RemindAt = x.RemindAt,
                    ReminderType = x.ReminderType ?? "",
                    IsSent = x.IsSent ?? false
                }).ToList();
        }

        public async Task<ReminderResponseDTO> GetByIdAsync(Guid userId, Guid reminderId)
        {
            var reminder = await _reminderRepository
                .GetByIdAsync(reminderId);

            if (reminder == null)
            {
                throw new Exception("Reminder not found.");
            }

            if (reminder.Schedule.UserId != userId)
            {
                throw new Exception(
                    "You are not allowed to access this reminder.");
            }

            return new ReminderResponseDTO
            {
                ReminderId = reminder.ReminderId,
                ScheduleId = reminder.ScheduleId,
                ScheduleTitle = reminder.Schedule.Title,
                RemindAt = reminder.RemindAt,
                ReminderType = reminder.ReminderType ?? "",
                IsSent = reminder.IsSent ?? false
            };
        }

        public async Task<ReminderResponseDTO> UpdateAsync(
                Guid userId,
                Guid reminderId,
                UpdateReminderDTO request)
        {
            var reminder = await _reminderRepository
                .GetByIdAsync(reminderId);

            if (reminder == null)
            {
                throw new Exception("Reminder not found.");
            }

            if (reminder.Schedule.UserId != userId)
            {
                throw new Exception(
                    "You are not allowed to update this reminder.");
            }

            if (request.RemindAt.ToUniversalTime()
                >= reminder.Schedule.StartTime)
            {
                throw new Exception(
                    "Reminder time must be before schedule start time.");
            }

            reminder.RemindAt =
                request.RemindAt.ToUniversalTime();

            reminder.ReminderType =
                request.ReminderType;

            await _reminderRepository.UpdateAsync(reminder);

            return new ReminderResponseDTO
            {
                ReminderId = reminder.ReminderId,
                ScheduleId = reminder.ScheduleId,
                ScheduleTitle = reminder.Schedule.Title,
                RemindAt = reminder.RemindAt,
                ReminderType = reminder.ReminderType ?? "",
                IsSent = reminder.IsSent ?? false
            };
        }

        public async Task DeleteAsync( Guid userId, Guid reminderId)
        {
            var reminder = await _reminderRepository
                .GetByIdAsync(reminderId);

            if (reminder == null)
            {
                throw new Exception("Reminder not found.");
            }

            if (reminder.Schedule.UserId != userId)
            {
                throw new Exception(
                    "You are not allowed to delete this reminder.");
            }

            await _reminderRepository.DeleteAsync(reminder);
        }
    }
}