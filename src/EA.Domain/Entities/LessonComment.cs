namespace EA.Domain.Entities;

public class LessonComment : BaseEntity
{
    public Guid LessonId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;

    public Lesson Lesson { get; set; } = null!;
    public ApplicationUser User { get; set; } = null!;
}
