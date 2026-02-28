namespace EA.Domain.Entities;

public class SpeakingAttempt : BaseEntity
{
    public string UserId { get; set; } = string.Empty;
    public Guid ExerciseId { get; set; }
    public string? AudioBlobUrl { get; set; }
    public string? TranscriptText { get; set; }
    public double AccuracyScore { get; set; }
    public double ProsodyScore { get; set; }
    public double CompletenessScore { get; set; }
    public double OverallScore { get; set; }
    public string? FeedbackJson { get; set; }

    public ApplicationUser User { get; set; } = null!;
    public Exercise Exercise { get; set; } = null!;
}
