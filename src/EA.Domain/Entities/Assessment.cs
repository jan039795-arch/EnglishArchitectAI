using EA.Domain.Enums;

namespace EA.Domain.Entities;

public class Assessment : BaseEntity
{
    public AssessmentScopeType ScopeType { get; set; }
    public Guid ScopeId { get; set; }
    public string Title { get; set; } = string.Empty;
    public int PassScore { get; set; }
    public int TimeLimitMinutes { get; set; }
    public bool CEFRAligned { get; set; }

    public ICollection<AssessmentResult> Results { get; set; } = [];
}
