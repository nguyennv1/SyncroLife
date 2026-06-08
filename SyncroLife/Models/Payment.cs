using System;
using System.Collections.Generic;

namespace SyncroLife.Models;

public partial class Payment
{
    public Guid PaymentId { get; set; }

    public Guid UserSubId { get; set; }

    public decimal Amount { get; set; }

    public string PaymentMethod { get; set; } = null!;

    public string Status { get; set; } = null!;

    public string? TransactionId { get; set; }

    public string? ErrorMessage { get; set; }

    public int? RetryCount { get; set; }

    public DateTime? PaymentDate { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual UserSubscription UserSub { get; set; } = null!;
}
