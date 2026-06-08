namespace SyncroLife.DTOs.User;

public class UserProfileDTO
{
    public Guid UserId { get; set; }

    public string Username { get; set; } = null!;

    //public string? Email { get; set; }

    public string? FullName { get; set; }

    public DateOnly? DateOfBirth { get; set; }

    public string? Gender { get; set; }

    public string RoleName { get; set; } = null!;
}