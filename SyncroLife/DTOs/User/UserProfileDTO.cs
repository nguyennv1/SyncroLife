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

    public decimal? Height { get; set; }

    public decimal? Weight { get; set; }

    public int? TargetCalories { get; set; }

    public decimal? MonthlyBudget { get; set; }

    public string? Allergies { get; set; }

    public string? SubscriptionType { get; set; }

    public DateTime? SubscriptionExpiry { get; set; }
}