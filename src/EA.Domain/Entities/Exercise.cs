using EA.Domain.Enums;

namespace EA.Domain.Entities;

public class Exercise : BaseEntity
{
    public Guid LessonId { get; set; }
    public ExerciseType Type { get; set; }
    public string Prompt { get; set; } = string.Empty;
    public string CorrectAnswer { get; set; } = string.Empty;
    public int Difficulty { get; set; }
    public string? Tags { get; set; }
    public ExerciseSource Source { get; set; }
    public bool IsValidated { get; set; }
    public string? CacheKey { get; set; }

    public Lesson Lesson { get; set; } = null!;
    public ICollection<ExerciseOption> Options { get; set; } = [];
    public ICollection<UserResponse> Responses { get; set; } = [];
    public ICollection<SpacedRepetitionCard> SpacedRepetitionCards { get; set; } = [];
}
