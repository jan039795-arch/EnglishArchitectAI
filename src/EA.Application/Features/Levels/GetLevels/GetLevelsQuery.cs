using MediatR;

namespace EA.Application.Features.Levels.GetLevels;

public record GetLevelsQuery : IRequest<List<LevelDto>>;
