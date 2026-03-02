using EA.Application.Contracts;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EA.Application.Features.Lessons.SearchLessons;

public class SearchLessonsQueryHandler : IRequestHandler<SearchLessonsQuery, List<SearchLessonDto>>
{
    private readonly IApplicationDbContext _context;

    public SearchLessonsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<SearchLessonDto>> Handle(SearchLessonsQuery request, CancellationToken cancellationToken)
    {
        return await _context.Lessons
            .Include(l => l.Module)
            .ThenInclude(m => m.Level)
            .Where(l => l.Title.Contains(request.Term, StringComparison.OrdinalIgnoreCase))
            .OrderBy(l => l.Title)
            .Take(20)
            .Select(l => new SearchLessonDto(
                l.Id,
                l.Title,
                l.Module.Title,
                l.Module.Level.Code,
                l.Module.Id))
            .ToListAsync(cancellationToken);
    }
}
