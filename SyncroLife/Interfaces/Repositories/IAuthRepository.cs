using SyncroLife.Models;

namespace SyncroLife.Interfaces.Repositories;

public interface IAuthRepository
{
    Task<User?> GetByUsernameAsync(string username);

    Task<User> CreateUserAsync(User user);
    Task<User?> GetByGoogleIdAsync(string googleId);
    Task<User> UpdateUserAsync(User user);
    Task<Role?> GetRoleByNameAsync(string roleName);
}