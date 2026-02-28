using EA.Application.Contracts;
using EA.Domain.Entities;
using MediatR;

namespace EA.Application.Features.Progress.CompleteLesson;

public class CompleteLessonCommandHandler : IRequestHandler<CompleteLessonCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public CompleteLessonCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(CompleteLessonCommand request, CancellationToken cancellationToken)
    {
        var progress = new UserProgress
        {
            UserId = request.UserId,
            LessonId = request.LessonId,
            Score = request.Score,
            TimeSpentSeconds = request.TimeSpentSeconds,
            CompletedAt = DateTime.UtcNow
        };

        _context.UserProgresses.Add(progress);
        await _context.SaveChangesAsync(cancellationToken);

        return progress.Id;
    }
}
