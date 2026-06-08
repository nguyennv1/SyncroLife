using System;
using System.Collections.Generic;

namespace SyncroLife.Models;

public partial class UserSubscription
{
    public Guid UserSubId { get; set; }

    public Guid UserId { get; set; }

    public Guid PlanId { get; set; }

    public DateOnly StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public string Status { get; set; } = null!;

    public bool? AutoRenew { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual SubscriptionPlan Plan { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
