using Microsoft.EntityFrameworkCore;
using SyncroLife.Data;
using SyncroLife.Interfaces.Repositories;
using SyncroLife.Models;

namespace SyncroLife.Repositories;

public class AuthRepository : IAuthRepository
{
    private readonly SyncroLifeDbContext _context;

    public AuthRepository(SyncroLifeDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        return await _context.Users
            .Include(x => x.Role)
            .FirstOrDefaultAsync(x => x.Username == username);
    }

    public async Task<User> CreateUserAsync(User user)
    {
        try
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }
        catch (Exception ex)
        {
            throw new Exception(ex.InnerException?.Message ?? ex.Message);
        }
    }

    public async Task<Role?> GetRoleByNameAsync(string roleName)
    {
        return await _context.Roles
        .FirstOrDefaultAsync(r => r.RoleName == roleName);
    }
}