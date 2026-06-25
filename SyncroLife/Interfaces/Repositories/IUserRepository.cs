using SyncroLife.Models;

namespace SyncroLife.Interfaces.Repositories;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid userId);

    Task UpdateAsync(User user);
    Task<List<User>> GetAllUsersAsync();
    Task UpdateDietaryPreferencesAsync(Guid userId, string allergiesCsv);

}