using SyncroLife.Interfaces.Repositories;
using SyncroLife.Interfaces.Services;

namespace SyncroLife.BackgroundServices
{
    public class ReminderBackgroundService
        : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public ReminderBackgroundService(
            IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();

                    var reminderRepository = scope.ServiceProvider.GetRequiredService<IReminderRepository>();
                    var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

                    var reminders = await reminderRepository.GetPendingRemindersAsync(DateTime.UtcNow);

                    if (reminders.Count > 0)
                    {
                        Console.WriteLine($"Found {reminders.Count} pending reminders.");
                    }

                    foreach (var reminder in reminders)
                    {
                        try
                        {
                            await notificationService.CreateReminderNotificationAsync(reminder);

                            reminder.IsSent = true;

                            await reminderRepository.UpdateAsync(reminder);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Reminder Error: {ex.Message}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Reminder Service Error: {ex.Message}");
                }

                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }
    }
}