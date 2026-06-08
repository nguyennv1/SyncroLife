using System;
using System.Collections.Generic;

namespace SyncroLife.Models;

public partial class UserDietaryPreference
{
    public Guid UserDietaryId { get; set; }

    public Guid UserId { get; set; }

    public Guid PreferenceId { get; set; }

    public DateTime? AddedAt { get; set; }

    public virtual DietaryPreference Preference { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
