namespace EA.Domain.Entities;

public class AssessmentResult : BaseEntity
{
    public string UserId { get; set; } = string.Empty;
    public Guid AssessmentId { get; set; }
    public int Score { get; set; }
    public bool IsPassed { get; set; }
    public DateTime CompletedAt { get; set; }
    public string? AIFeedbackJson { get; set; }

    public ApplicationUser User { get; set; } = null!;
    public Assessment Assessment { get; set; } = null!;
}
