using System;
using System.Collections.Generic;

namespace SyncroLife.Models;

public partial class ActiveSubscription
{
    public Guid? UserSubId { get; set; }

    public Guid? UserId { get; set; }

    public string? Username { get; set; }

    public string? Email { get; set; }

    public string? PlanName { get; set; }

    public decimal? Price { get; set; }

    public DateOnly? StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public int? DaysRemaining { get; set; }
}
