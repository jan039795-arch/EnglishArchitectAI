using EA.Domain.Enums;

namespace EA.Domain.Entities;

public class Subscription : BaseEntity
{
    public string UserId { get; set; } = string.Empty;
    public SubscriptionPlan Plan { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string? MPSubscriptionId { get; set; }
    public SubscriptionStatus Status { get; set; }

    public ApplicationUser User { get; set; } = null!;
}
