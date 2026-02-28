using EA.Application.Contracts;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EA.Application.Features.Progress.GetUserProgress;

public class GetUserProgressQueryHandler : IRequestHandler<GetUserProgressQuery, List<UserProgressDto>>
{
    private readonly IApplicationDbContext _context;

    public GetUserProgressQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<UserProgressDto>> Handle(GetUserProgressQuery request, CancellationToken cancellationToken)
    {
        return await _context.UserProgresses
            .Where(p => p.UserId == request.UserId)
            .Include(p => p.Lesson)
            .Select(p => new UserProgressDto(p.Id, p.UserId, p.LessonId, p.Lesson.Title, p.CompletedAt, p.Score, p.TimeSpentSeconds))
            .ToListAsync(cancellationToken);
    }
}
