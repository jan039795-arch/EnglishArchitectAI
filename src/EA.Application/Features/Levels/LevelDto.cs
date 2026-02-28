namespace EA.Application.Features.Levels;

public record LevelDto(Guid Id, string Code, string Name, int Order, string? UnlockRequirement);
