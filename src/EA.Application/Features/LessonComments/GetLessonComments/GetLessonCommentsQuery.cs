using MediatR;

namespace EA.Application.Features.LessonComments.GetLessonComments;

public record GetLessonCommentsQuery(Guid LessonId) : IRequest<List<LessonCommentDto>>;
