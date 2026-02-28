using MediatR;

namespace EA.Application.Features.Lessons.GetLessonById;

public record GetLessonByIdQuery(Guid Id) : IRequest<LessonDetailDto?>;
