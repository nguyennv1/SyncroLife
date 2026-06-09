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
                Console.WriteLine("ReminderBackgroundService Running...");
                try
                {
                    using var scope =
                        _scopeFactory.CreateScope();

                    var reminderRepository =
                        scope.ServiceProvider
                            .GetRequiredService<IReminderRepository>();

                    var notificationService =
                        scope.ServiceProvider
                            .GetRequiredService<INotificationService>();

                    var reminders =
                        await reminderRepository
                            .GetPendingRemindersAsync(
                                DateTime.UtcNow);

                    foreach (var reminder in reminders)
                    {
                        try
                        {
                            await notificationService
                                .CreateReminderNotificationAsync(
                                    reminder);

                            reminder.IsSent = true;

                            await reminderRepository
                                .UpdateAsync(reminder);
                        }
                        catch
                        {
                        }
                    }
                }
                catch
                {
                }

                await Task.Delay(
                    TimeSpan.FromMinutes(1),
                    stoppingToken);
            }
        }
    }
}