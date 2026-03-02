namespace EA.Application.Features.LessonComments;

public record LessonCommentDto(Guid Id, string Body, string Username, DateTime CreatedAt);
