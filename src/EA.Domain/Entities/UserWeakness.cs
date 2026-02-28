namespace EA.Domain.Entities;

public class UserWeakness : BaseEntity
{
    public string UserId { get; set; } = string.Empty;
    public string Tag { get; set; } = string.Empty;
    public double WeaknessScore { get; set; }
    public int TotalAttempts { get; set; }
    public int FailCount { get; set; }
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

    public ApplicationUser User { get; set; } = null!;
}
