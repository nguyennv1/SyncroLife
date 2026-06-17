using SyncroLife.Interfaces.Repositories;
using SyncroLife.Interfaces.Services;

namespace SyncroLife.Services.BackgroundServices;

public class RecommendationBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public RecommendationBackgroundService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _scopeFactory.CreateScope();

            var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
            var recommendationService = scope.ServiceProvider.GetRequiredService<IRecommendationService>();

            var users = await userRepository.GetAllUsersAsync();

            foreach (var user in users)
            {
                try
                {
                    await recommendationService.GenerateForUserAsync(user.UserId);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Recommendation Error: {ex.Message}");
                }
            }

            await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
        }
    }
}