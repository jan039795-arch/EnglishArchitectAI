using MediatR;

namespace EA.Application.Features.Modules.GetModulesByLevel;

public record GetModulesByLevelQuery(Guid LevelId) : IRequest<List<ModuleDto>>;
