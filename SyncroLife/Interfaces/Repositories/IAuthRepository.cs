using SyncroLife.Models;

namespace SyncroLife.Interfaces.Repositories;

public interface IAuthRepository
{
    Task<User?> GetByUsernameAsync(string username);

    Task<User> CreateUserAsync(User user);
    Task<Role?> GetRoleByNameAsync(string roleName);
}