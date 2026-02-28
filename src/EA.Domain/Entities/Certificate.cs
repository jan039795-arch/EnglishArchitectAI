namespace EA.Domain.Entities;

public class Certificate : BaseEntity
{
    public string UserId { get; set; } = string.Empty;
    public string LevelCode { get; set; } = string.Empty;
    public DateTime IssuedAt { get; set; }
    public string? PDFBlobUrl { get; set; }
    public Guid VerificationCode { get; set; } = Guid.NewGuid();

    public ApplicationUser User { get; set; } = null!;
}
