namespace SyncroLife.DTOs.User;

public class UserResponseDTO
{
    public Guid UserId { get; set; }

    public string Username { get; set; } = null!;

    public string? FullName { get; set; }

    public string? Email { get; set; }

    public string? Gender { get; set; }

    public DateOnly? DateOfBirth { get; set; }
    public string? Role { get; set; }
    public bool IsDeleted { get; set; }
}