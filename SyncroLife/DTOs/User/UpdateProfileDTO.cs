namespace SyncroLife.DTOs.User;

public class UpdateProfileDTO
{
    //public string? Email { get; set; }

    public string? FullName { get; set; }

    public DateOnly? DateOfBirth { get; set; }

    public string? Gender { get; set; }

    public decimal? Height { get; set; }

    public decimal? Weight { get; set; }

    public int? TargetCalories { get; set; }

    public decimal? MonthlyBudget { get; set; }

    public string? Allergies { get; set; }
}