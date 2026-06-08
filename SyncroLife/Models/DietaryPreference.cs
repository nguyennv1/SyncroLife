using System;
using System.Collections.Generic;

namespace SyncroLife.Models;

public partial class DietaryPreference
{
    public Guid PreferenceId { get; set; }

    public string PreferenceName { get; set; } = null!;

    public string? Description { get; set; }

    public string? PreferenceType { get; set; }

    public bool? IsActive { get; set; }

    public bool? IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<AiRecommendation> AiRecommendations { get; set; } = new List<AiRecommendation>();

    public virtual ICollection<UserDietaryPreference> UserDietaryPreferences { get; set; } = new List<UserDietaryPreference>();
}
