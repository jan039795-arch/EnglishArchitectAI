namespace EA.Application.Features.Modules;

public record ModuleDto(Guid Id, Guid LevelId, string Title, string Description, int Order, string? YoutubePlaylistId, int EstimatedHours);
