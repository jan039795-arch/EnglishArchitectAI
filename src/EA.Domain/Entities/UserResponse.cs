namespace EA.Domain.Entities;

public class UserResponse : BaseEntity
{
    public string UserId { get; set; } = string.Empty;
    public Guid ExerciseId { get; set; }
    public string UserAnswer { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
    public int TimeTakenSeconds { get; set; }
    public string? AIFeedback { get; set; }

    public ApplicationUser User { get; set; } = null!;
    public Exercise Exercise { get; set; } = null!;
}
