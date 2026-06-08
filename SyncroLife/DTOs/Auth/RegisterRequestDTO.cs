namespace SyncroLife.DTOs.Auth;

public class RegisterRequestDTO
{
    public string Username { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string ConfirmPassword { get; set; } = null!;
}