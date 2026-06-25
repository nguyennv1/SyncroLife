using SyncroLife.DTOs.User;
using SyncroLife.Interfaces.Repositories;
using SyncroLife.Interfaces.Services;

namespace SyncroLife.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<List<UserResponseDTO>> GetAllUsersAsync()
    {
        var users =
        await _userRepository.GetAllUsersAsync();

        return users.Select(u => new UserResponseDTO
        {
            UserId = u.UserId,
            Username = u.Username,
            Email = u.Email,
            FullName = u.FullName,
            Gender = u.Gender == "M" ? "Male" : (u.Gender == "F" ? "Female" : u.Gender),
            DateOfBirth = u.DateOfBirth,
            Role = u.Role.RoleName,
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
            Allergies = user.Allergies
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