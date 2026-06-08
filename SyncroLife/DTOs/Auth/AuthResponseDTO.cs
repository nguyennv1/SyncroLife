using SyncroLife.DTOs.User;

namespace SyncroLife.DTOs.Auth;

public class AuthResponseDTO
{
    public string Token { get; set; } = null!;

    public UserResponseDTO User { get; set; } = null!;
}