using EA.Domain.Enums;

namespace EA.Domain.Entities;

public class Lesson : BaseEntity
{
    public Guid ModuleId { get; set; }
    public string Title { get; set; } = string.Empty;
    public SkillType SkillType { get; set; }
    public int Order { get; set; }
    public string? ContentJson { get; set; }
    public bool IsAIGenerated { get; set; }

    public Module Module { get; set; } = null!;
    public ICollection<Exercise> Exercises { get; set; } = [];
    public ICollection<UserProgress> Progresses { get; set; } = [];
}
