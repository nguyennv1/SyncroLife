namespace SyncroLife.DTOs.User;

public class UpdateProfileDTO
{
    //public string? Email { get; set; }

    public string? FullName { get; set; }

    public DateOnly? DateOfBirth { get; set; }

    public string? Gender { get; set; }
}