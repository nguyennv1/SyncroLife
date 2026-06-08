using SyncroLife.DTOs.Auth;
using SyncroLife.DTOs.User;

namespace SyncroLife.Interfaces.Services;

public interface IAuthService
{
    Task<UserResponseDTO> RegisterAsync(RegisterRequestDTO request);
    Task<LoginResponseDTO> LoginAsync(LoginRequestDTO request);
}