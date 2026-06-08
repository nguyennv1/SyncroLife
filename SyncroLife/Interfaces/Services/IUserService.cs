using SyncroLife.DTOs.User;

namespace SyncroLife.Interfaces.Services;

public interface IUserService
{
    Task<UserProfileDTO> GetProfileAsync(Guid userId);

    Task<UserProfileDTO> UpdateProfileAsync(Guid userId,UpdateProfileDTO request);
    Task<List<UserResponseDTO>> GetAllUsersAsync();
}