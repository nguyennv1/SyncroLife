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
            Gender = u.Gender,
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
            //Email = user.Email,
            FullName = user.FullName,
            DateOfBirth = user.DateOfBirth,
            Gender = user.Gender,
            RoleName = user.Role.RoleName
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

        //user.Email = request.Email;
        user.FullName = request.FullName;
        user.DateOfBirth = request.DateOfBirth;
        user.Gender = request.Gender;

        await _userRepository.UpdateAsync(user);

        return await GetProfileAsync(userId);
    }
}