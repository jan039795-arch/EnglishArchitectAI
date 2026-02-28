namespace EA.Application.Features.Progress;

public record UserProgressDto(Guid Id, string UserId, Guid LessonId, string LessonTitle, DateTime CompletedAt, int Score, int TimeSpentSeconds);
