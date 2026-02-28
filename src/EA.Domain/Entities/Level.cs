namespace EA.Domain.Entities;

public class Level : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int Order { get; set; }
    public string? UnlockRequirement { get; set; }

    public ICollection<Module> Modules { get; set; } = [];
}
