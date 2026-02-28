namespace EA.Domain.Entities;

public class ExerciseOption : BaseEntity
{
    public Guid ExerciseId { get; set; }
    public string Text { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
    public string? Explanation { get; set; }

    public Exercise Exercise { get; set; } = null!;
}
