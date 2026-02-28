using EA.Application.Contracts;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EA.Application.Features.Lessons.GetLessonsByModule;

public class GetLessonsByModuleQueryHandler : IRequestHandler<GetLessonsByModuleQuery, List<LessonDto>>
{
    private readonly IApplicationDbContext _context;

    public GetLessonsByModuleQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<LessonDto>> Handle(GetLessonsByModuleQuery request, CancellationToken cancellationToken)
    {
        return await _context.Lessons
            .Where(l => l.ModuleId == request.ModuleId)
            .OrderBy(l => l.Order)
            .Select(l => new LessonDto(l.Id, l.ModuleId, l.Title, l.SkillType.ToString(), l.Order, l.IsAIGenerated))
            .ToListAsync(cancellationToken);
    }
}
