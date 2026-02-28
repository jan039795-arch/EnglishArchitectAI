namespace EA.Domain.Entities;

public class Module : BaseEntity
{
    public Guid LevelId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Order { get; set; }
    public string? YoutubePlaylistId { get; set; }
    public int EstimatedHours { get; set; }

    public Level Level { get; set; } = null!;
    public ICollection<Lesson> Lessons { get; set; } = [];
}
