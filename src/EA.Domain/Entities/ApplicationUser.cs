using EA.Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace EA.Domain.Entities;

public class ApplicationUser : IdentityUser
{
    public CEFRLevel CEFRLevel { get; set; } = CEFRLevel.A1;
    public bool PlacementCompleted { get; set; }
    public string? AvatarUrl { get; set; }
    public DateTime? LastActiveAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Subscription> Subscriptions { get; set; } = [];
    public ICollection<UserProgress> Progresses { get; set; } = [];
    public ICollection<UserWeakness> Weaknesses { get; set; } = [];
    public ICollection<UserResponse> Responses { get; set; } = [];
    public ICollection<SpacedRepetitionCard> SpacedRepetitionCards { get; set; } = [];
    public ICollection<AssessmentResult> AssessmentResults { get; set; } = [];
    public ICollection<SpeakingAttempt> SpeakingAttempts { get; set; } = [];
    public ICollection<Certificate> Certificates { get; set; } = [];
}
