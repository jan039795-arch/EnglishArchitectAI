namespace EA.Domain.Entities;

public class UserProgress : BaseEntity
{
    public string UserId { get; set; } = string.Empty;
    public Guid LessonId { get; set; }
    public DateTime CompletedAt { get; set; }
    public int Score { get; set; }
    public int TimeSpentSeconds { get; set; }

    public ApplicationUser User { get; set; } = null!;
    public Lesson Lesson { get; set; } = null!;
}
