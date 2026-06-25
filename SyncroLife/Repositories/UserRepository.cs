using Microsoft.EntityFrameworkCore;
using SyncroLife.Data;
using SyncroLife.Interfaces.Repositories;
using SyncroLife.Models;

namespace SyncroLife.Repositories;

public class UserRepository : IUserRepository
{
    private readonly SyncroLifeDbContext _context;

    public UserRepository(SyncroLifeDbContext context)
    {
        _context = context;
    }

    public async Task<List<User>> GetAllUsersAsync()
    {
        return await _context.Users
            .Include(x => x.Role)
            .ToListAsync();
    }

    public async Task<User?> GetByIdAsync(Guid userId)
    {
        return await _context.Users
            .Include(x => x.Role)
            .Include(x => x.UserDietaryPreferences)
                .ThenInclude(udp => udp.Preference)
            .FirstOrDefaultAsync(x => x.UserId == userId);
    }

    public async Task UpdateAsync(User user)
    {
        _context.Users.Update(user);

        await _context.SaveChangesAsync();
    }

    public async Task UpdateDietaryPreferencesAsync(Guid userId, string allergiesCsv)
    {
        var user = await GetByIdAsync(userId);
        if (user == null) return;

        var newNames = (allergiesCsv ?? "")
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(x => x.Trim())
            .Where(x => !string.IsNullOrEmpty(x))
            .ToList();

        // 1. Remove links that are not in the new list (or are orphans/corrupted)
        var toRemove = user.UserDietaryPreferences
            .Where(udp => udp.Preference == null || !newNames.Any(n => n.Equals(udp.Preference.PreferenceName, StringComparison.OrdinalIgnoreCase)))
            .ToList();

        foreach (var link in toRemove)
        {
            _context.UserDietaryPreferences.Remove(link);
        }

        // 2. Add new links
        foreach (var name in newNames)
        {
            // Find DietaryPreference by name (even if soft-deleted or inactive)
            var pref = await _context.DietaryPreferences
                .FirstOrDefaultAsync(dp => dp.PreferenceName.ToLower() == name.ToLower());

            if (pref == null)
            {
                pref = new DietaryPreference
                {
                    PreferenceId = Guid.NewGuid(),
                    PreferenceName = name,
                    PreferenceType = "allergy",
                    IsActive = true,
                    IsDeleted = false,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                _context.DietaryPreferences.Add(pref);
                // Save to generate key before linking
                await _context.SaveChangesAsync();
            }
            else if (pref.IsDeleted == true || pref.IsActive != true)
            {
                pref.IsDeleted = false;
                pref.IsActive = true;
                pref.UpdatedAt = DateTime.UtcNow;
                _context.DietaryPreferences.Update(pref);
                await _context.SaveChangesAsync();
            }

            // Verify existence using IDs directly to avoid null navigation checks on newly added untracked links
            var exists = user.UserDietaryPreferences
                .Any(udp => udp.PreferenceId == pref.PreferenceId);

            if (!exists)
            {
                var link = new UserDietaryPreference
                {
                    UserDietaryId = Guid.NewGuid(),
                    UserId = userId,
                    PreferenceId = pref.PreferenceId,
                    AddedAt = DateTime.UtcNow
                };
                _context.UserDietaryPreferences.Add(link);
            }
        }

        await _context.SaveChangesAsync();
    }
}