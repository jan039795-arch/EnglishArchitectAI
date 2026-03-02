using EA.Application.Contracts;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EA.Application.Features.LessonComments.GetLessonComments;

public class GetLessonCommentsQueryHandler : IRequestHandler<GetLessonCommentsQuery, List<LessonCommentDto>>
{
    private readonly IApplicationDbContext _context;

    public GetLessonCommentsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<LessonCommentDto>> Handle(GetLessonCommentsQuery request, CancellationToken cancellationToken)
    {
        return await _context.LessonComments
            .Where(c => c.LessonId == request.LessonId)
            .Include(c => c.User)
            .OrderBy(c => c.CreatedAt)
            .Select(c => new LessonCommentDto(
                c.Id,
                c.Body,
                c.User.UserName ?? "Anonymous",
                c.CreatedAt))
            .ToListAsync(cancellationToken);
    }
}
