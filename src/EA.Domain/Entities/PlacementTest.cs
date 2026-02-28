using EA.Domain.Enums;

namespace EA.Domain.Entities;

public class PlacementTest : BaseEntity
{
    public string UserId { get; set; } = string.Empty;
    public CEFRLevel FinalLevel { get; set; }
    public int TotalQuestions { get; set; }
    public string? ResponsesJson { get; set; }
    public DateTime CompletedAt { get; set; }

    public ApplicationUser User { get; set; } = null!;
}
