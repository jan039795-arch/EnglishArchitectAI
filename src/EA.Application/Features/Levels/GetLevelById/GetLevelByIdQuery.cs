using MediatR;

namespace EA.Application.Features.Levels.GetLevelById;

public record GetLevelByIdQuery(Guid Id) : IRequest<LevelDto?>;
