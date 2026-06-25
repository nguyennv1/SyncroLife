namespace SyncroLife.DTOs.Auth;

public class GoogleLoginRequestDTO
{
    public string ServerAuthCode { get; set; } = null!;
    public string? Email { get; set; }
    public string? FullName { get; set; }
    public string? ClientId { get; set; }
    public string? RedirectUri { get; set; }
}
