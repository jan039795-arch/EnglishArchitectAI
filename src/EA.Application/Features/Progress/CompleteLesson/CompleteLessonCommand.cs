using MediatR;

namespace EA.Application.Features.Progress.CompleteLesson;

public record CompleteLessonCommand(
    string UserId,
    Guid LessonId,
    int Score,
    int TimeSpentSeconds) : IRequest<Guid>;
