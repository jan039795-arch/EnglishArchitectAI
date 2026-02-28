using MediatR;

namespace EA.Application.Features.Exercises.GetExercisesByLesson;

public record GetExercisesByLessonQuery(Guid LessonId) : IRequest<List<ExerciseDto>>;
