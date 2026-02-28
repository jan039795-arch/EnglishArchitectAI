using MediatR;

namespace EA.Application.Features.Lessons.GetLessonsByModule;

public record GetLessonsByModuleQuery(Guid ModuleId) : IRequest<List<LessonDto>>;
