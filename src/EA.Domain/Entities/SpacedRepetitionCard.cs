namespace EA.Domain.Entities;

public class SpacedRepetitionCard : BaseEntity
{
    public string UserId { get; set; } = string.Empty;
    public Guid ExerciseId { get; set; }
    public double EasinessFactor { get; set; } = 2.5;
    public int Interval { get; set; }
    public int Repetitions { get; set; }
    public DateTime NextReviewDate { get; set; } = DateTime.UtcNow;

    public ApplicationUser User { get; set; } = null!;
    public Exercise Exercise { get; set; } = null!;
}
