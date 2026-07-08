using Microsoft.EntityFrameworkCore;
using SyncroLife.Data;
using SyncroLife.DTOs.User;
using SyncroLife.Interfaces.Repositories;
using SyncroLife.Interfaces.Services;

namespace SyncroLife.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly SyncroLifeDbContext _context;

    public UserService(IUserRepository userRepository, SyncroLifeDbContext context)
    {
        _userRepository = userRepository;
        _context = context;
    }

    public async Task<List<UserResponseDTO>> GetAllUsersAsync()
    {
        var users = await _userRepository.GetAllUsersAsync();

        // Query all active subscriptions in one database round-trip
        var activeSubs = await _context.UserSubscriptions
            .Include(us => us.Plan)
            .Where(us => us.Status == "active")
            .ToListAsync();

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var expiredSubs = activeSubs.Where(us => us.EndDate.HasValue && us.EndDate.Value < today).ToList();
        if (expiredSubs.Any())
        {
            foreach (var sub in expiredSubs)
            {
                sub.Status = "expired";
                sub.UpdatedAt = DateTime.UtcNow;
                activeSubs.Remove(sub);
            }
            await _context.SaveChangesAsync();
        }

        var subMap = activeSubs.ToDictionary(us => us.UserId, us => us.Plan?.PlanName ?? "Free");

        return users.Select(u => new UserResponseDTO
        {
            UserId = u.UserId,
            Username = u.Username,
            Email = u.Email,
            FullName = u.FullName,
            Gender = u.Gender == "M" ? "Male" : (u.Gender == "F" ? "Female" : u.Gender),
            DateOfBirth = u.DateOfBirth,
            Role = u.Role.RoleName,
            SubscriptionPlan = subMap.ContainsKey(u.UserId) ? subMap[u.UserId] : "Free",
            IsDeleted = u.IsDeleted ?? false
        }).ToList();
    }

    public async Task<UserProfileDTO> GetProfileAsync(Guid userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);

        if (user == null)
        {
            throw new Exception("User not found");
        }

        string? subscriptionType = "FREE";
        DateTime? subscriptionExpiry = null;

        // Query the active subscription directly from the context to bypass EF Core navigation caching
        var activeSub = await _context.UserSubscriptions
            .Include(us => us.Plan)
            .FirstOrDefaultAsync(us => us.UserId == userId && us.Status == "active");

        if (activeSub != null)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            if (activeSub.EndDate.HasValue && activeSub.EndDate.Value < today)
            {
                activeSub.Status = "expired";
                activeSub.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                activeSub = null;
            }
        }

        if (activeSub != null)
        {
            subscriptionType = activeSub.Plan?.PlanName ?? "FREE";
            if (activeSub.EndDate.HasValue)
            {
                subscriptionExpiry = activeSub.EndDate.Value.ToDateTime(TimeOnly.MinValue);
            }
        }

        return new UserProfileDTO
        {
            UserId = user.UserId,
            Username = user.Username,
            FullName = user.FullName,
            DateOfBirth = user.DateOfBirth,
            Gender = user.Gender == "M" ? "Male" : (user.Gender == "F" ? "Female" : user.Gender),
            RoleName = user.Role.RoleName,
            Height = user.Height,
            Weight = user.Weight,
            TargetCalories = user.TargetCalories,
            MonthlyBudget = user.MonthlyBudget,
            Allergies = user.Allergies,
            SubscriptionType = subscriptionType,
            SubscriptionExpiry = subscriptionExpiry
        };
    }

    public async Task<UserProfileDTO> UpdateProfileAsync(
        Guid userId,
        UpdateProfileDTO request)
    {
        var user = await _userRepository.GetByIdAsync(userId);

        if (user == null)
        {
            throw new Exception("User not found");
        }

        string? mappedGender = null;
        if (!string.IsNullOrEmpty(request.Gender))
        {
            var g = request.Gender.Trim().ToLower();
            if (g == "male" || g == "m" || g == "nam")
            {
                mappedGender = "M";
            }
            else if (g == "female" || g == "f" || g == "nữ")
            {
                mappedGender = "F";
            }
            else
            {
                mappedGender = request.Gender.Length > 0 ? request.Gender.Substring(0, 1).ToUpper() : null;
            }
        }

        user.FullName = request.FullName;
        user.DateOfBirth = request.DateOfBirth;
        user.Gender = mappedGender;
        user.Height = request.Height;
        user.Weight = request.Weight;
        user.TargetCalories = request.TargetCalories;
        user.MonthlyBudget = request.MonthlyBudget;

        await _userRepository.UpdateAsync(user);
        await _userRepository.UpdateDietaryPreferencesAsync(userId, request.Allergies ?? "");

        return await GetProfileAsync(userId);
    }
}