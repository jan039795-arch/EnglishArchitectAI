using EA.Application.Contracts;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EA.Application.Features.Lessons.GetLessonById;

public class GetLessonByIdQueryHandler : IRequestHandler<GetLessonByIdQuery, LessonDetailDto?>
{
    private readonly IApplicationDbContext _context;

    public GetLessonByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<LessonDetailDto?> Handle(GetLessonByIdQuery request, CancellationToken cancellationToken)
    {
        return await _context.Lessons
            .Where(l => l.Id == request.Id)
            .Select(l => new LessonDetailDto(l.Id, l.ModuleId, l.Title, l.SkillType.ToString(), l.Order, l.IsAIGenerated, l.ContentJson))
            .FirstOrDefaultAsync(cancellationToken);
    }
}
