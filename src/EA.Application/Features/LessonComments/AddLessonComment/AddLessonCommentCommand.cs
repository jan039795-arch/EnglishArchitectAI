using MediatR;

namespace EA.Application.Features.LessonComments.AddLessonComment;

public record AddLessonCommentCommand(Guid LessonId, string UserId, string Body) : IRequest<LessonCommentDto>;
