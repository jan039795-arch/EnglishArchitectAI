using MediatR;

namespace EA.Application.Features.Lessons.SearchLessons;

public record SearchLessonsQuery(string Term) : IRequest<List<SearchLessonDto>>;
