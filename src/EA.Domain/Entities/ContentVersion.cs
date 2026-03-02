namespace EA.Domain.Entities;

public class ContentVersion : BaseEntity
{
    public Guid ExerciseId { get; set; }
    public int VersionNumber { get; set; }
    public string OriginalPrompt { get; set; } = string.Empty;
    public string GeneratedPrompt { get; set; } = string.Empty;
    public string? ChangeLog { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? GeneratedAt { get; set; }

    public Exercise Exercise { get; set; } = null!;
}
