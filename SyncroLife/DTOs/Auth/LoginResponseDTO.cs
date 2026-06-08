namespace SyncroLife.DTOs.Auth;

public class LoginResponseDTO
{
    public Guid UserId { get; set; }

    public string Username { get; set; } = null!;

    public string Role { get; set; } = null!;

    public string Token { get; set; } = null!;
}